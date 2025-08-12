<?php
header("Access-Control-Allow-Origin: *");
@session_start();
$extensions_allowed = ['.json', '.jpg', '.png'];

if(!isset($_GET['action']))
	exit();

if($_GET['action'] == 'login')
{
	login(getParam('username'), getParam('password'));
}
else if($_GET['action'] == 'listFiles')
{
	listFiles(getParam('folderName'), getParam('extension', '.json'));
}
else if($_GET['action'] == 'load')
{
	loadFile(getParam('fileName'), getParam('folderName'), getParam('extension', '.json') );
}
else if($_GET['action'] == 'exists')
{
	fileExists(getParam('fileName'), getParam('folderName'), getParam('extension', '.json') );
}
else if($_GET['action'] == 'save')
{
	saveFile(getParam('fileName'), getParam('folderName'), getParam('extension', '.json'), getFile('file'));
}
else if($_GET['action'] == 'delete')
{
	deleteFile(getParam('fileName'), getParam('folderName'), getParam('extension', '.json') );
}
else if($_GET['action'] == 'rename')
{
	renameFile(getParam('fileName'), getParam('newFileName'), getParam('folderName'), getParam('extension', '.json'));
}
else if($_GET['action'] == 'copy')
{
	copyFile(getParam('fileName'), getParam('newFileName'), getParam('folderName'), getParam('extension', '.json'));
}

function login($username, $password)
{
	// implement your own logic to verify users
	if($username == 'demo' && $password == 'demo')
	{
		$_SESSION['user_folder'] = 'demo';
		jsonResult( ['success' => true ]);
	}
	jsonResult( ['success' => false, 'error'=> 'User not found' ]);
}

function fileExists($fileName, $folderName, $ext)
{
	$dir = GetUserFolder();
	$path = $dir . $folderName . DIRECTORY_SEPARATOR . $fileName . $ext;

	exit( file_exists($path) ? 'true': 'false');
}

function loadFile($fileName, $folderName, $ext)
{
	$dir = GetUserFolder();
	$path = $dir . $folderName . DIRECTORY_SEPARATOR . $fileName . $ext;

	if (!file_exists($path)) {
		jsonResult( ['success' => false, 'error'=> 'File not found:'.$folderName . DIRECTORY_SEPARATOR . $fileName . $ext ]);
	}

	$json  = file_get_contents($path);
	jsonResult( $json );
}

function listFiles($folderName, $ext)
{
	$dir = GetUserFolder();
	$path = $dir . $folderName . DIRECTORY_SEPARATOR;

	if (!file_exists($dir)) {
		@mkdir($dir, 0777, true);
	}
	if (!file_exists($path)) {
		@mkdir($path, 0777, true);
	}

	chdir($path);
	$files = glob("*{".$ext."}", GLOB_BRACE);

	jsonResult( ['list' => $files] );
}

function saveFile($fileName, $folderName, $ext, $tmpName)
{
	$dir = GetUserFolder();
	$dir2 = $dir . $folderName . DIRECTORY_SEPARATOR;
	$path = $dir2 . $fileName . $ext;


	if (!file_exists($dir)) {
		@mkdir($dir, 0777, true);
	}
	if (!file_exists($dir2)) {
		@mkdir($dir2, 0777, true);
	}
	if (move_uploaded_file($tmpName, $path)) {
		jsonResult(['success' => true]);
	} else {
		jsonResult(['success'=>false, 'error'=> 'Upload error']);
	}
}

function deleteFile($fileName, $folderName, $ext)
{
	$dir = GetUserFolder();
	$path = $dir . $folderName . DIRECTORY_SEPARATOR . $fileName . $ext;


	if (!file_exists($path)) {
		jsonResult( ['success' => false, 'error'=> 'File not found:'.$folderName . DIRECTORY_SEPARATOR . $fileName . $ext ]);
	}
	unlink($path);
	jsonResult(['success'=>true]);
}

function renameFile($fileName, $newFileName, $folderName, $ext)
{
	$dir = GetUserFolder();
	$path = $dir . $folderName . DIRECTORY_SEPARATOR . $fileName . $ext;
	$path2 = $dir . $folderName . DIRECTORY_SEPARATOR . $newFileName . $ext;

	if (!file_exists($path)) {
		jsonResult( ['success' => false, 'error'=> 'File not found:'.$folderName . DIRECTORY_SEPARATOR . $fileName . $ext ]);
	}
	rename($path, $path2);
	jsonResult(['success' => true]);
}

function copyFile($fileName, $newFileName, $folderName, $ext)
{
	$dir = GetUserFolder();
	$path = $dir . $folderName . DIRECTORY_SEPARATOR . $fileName . $ext;
	$path2 = $dir . $folderName . DIRECTORY_SEPARATOR . $newFileName . $ext;

	if (!file_exists($path)) {
		jsonResult( ['success' => false, 'error'=> 'File not found:'.$folderName . DIRECTORY_SEPARATOR . $fileName . $ext ]);
	}
	copy($path, $path2);
	jsonResult(['success' => true]);
}

function getUserFolder()
{
	if(!isset($_SESSION['user_folder']))
		jsonResult( ['success'=> false, 'error' => 'User not logged in' ]);

	return dirname(__FILE__).DIRECTORY_SEPARATOR.'UserFiles'.DIRECTORY_SEPARATOR.$_SESSION['user_folder'].DIRECTORY_SEPARATOR;
}

function jsonResult($data)
{
	header("Content-Type: application/json");
	echo is_array($data) ? json_encode($data) : $data;
	exit();
}
function getFile($name)
{
	for($i = 0; $i<count($_FILES); $i++)
	{
		if($_FILES[$i]['name'] == $name)
			return $_FILES[$i]['tmp_name'];
	}
	jsonResult( ['success'=> false, 'error' => 'File '.$name.' is missing' ]);
}
function getParam($name, $default = '')
{
	global $extensions_allowed;
	if(isset($_POST[$name]))
	{
		$val =  $_POST[$name];
		if($name == 'extension' && !in_array($val, $extensions_allowed ))
			jsonResult( ['success'=> false, 'error' => 'Extension is not allowed' ]);

		return $_POST[$name];
	} 
	if(empty($default))
		jsonResult( ['success'=> false, 'error' => 'Param '.$name.' is missing' ]);

	return $default;
}
