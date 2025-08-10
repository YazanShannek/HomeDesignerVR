using System;
using Exoa.Common;
using Exoa.Designer.Data;
using Exoa.Events;
using UnityEngine;
using static Exoa.Events.GameEditorEvents;

namespace Exoa.Designer
{
    /// <summary>
    /// Main controller class for the application, handling the application state, roof configuration, and various game events.
    /// </summary>
    public class AppController : MonoBehaviour
    {
        // Singleton instance of AppController
        public static AppController Instance;

        // Delegate for handling application state changes
        public delegate void OnAppStateChangeHandler(States state);

        // Event triggered on application state change
        public static OnAppStateChangeHandler OnAppStateChange;

        // List of modules received from the backend
        private ModuleDataModels.ModuleList backendModuleList;

        // Configuration properties for walls, doors, and windows
        public float wallsHeight = 3f; /// <summary>Height of the walls.</summary>
        public float doorsHeight = 2.5f; /// <summary>Height of the doors.</summary>
        public float interiorWallThickness = .05f; /// <summary>Thickness of interior walls.</summary>
        public float exteriorWallThickness = .1f; /// <summary>Thickness of exterior walls.</summary>
        public float windowsThickness = 0.06f; /// <summary>Thickness of windows.</summary>
        public float doorsThickness = 0.06f; /// <summary>Thickness of doors.</summary>

        // Configuration for the roof
        public RoofConfig roof;

        // Enumeration for the different states of the application
        public enum States { Idle, Draw, PreviewBuilding, PlayMode };

        // Current state of the application
        public States currentState;

        // Property for managing the current state and triggering state change events
        public States State
        {
            get => currentState;
            set
            {
                currentState = value;
                OnAppStateChange?.Invoke(value);
            }
        }

        // Enumeration for the different types of roofs
        public enum RoofType
        {
            Flat = 0,
            Hipped = 1,
            Gabled = 2,
        }

        /// <summary>
        /// Configuration class for roof settings, including type, thickness, and color.
        /// </summary>
        [Serializable]
        public class RoofConfig
        {
            public RoofType type = RoofType.Flat; /// <summary>Type of the roof.</summary>
            public float thickness = .2f; /// <summary>Thickness of the roof.</summary>
            public float overhang = .2f; /// <summary>Overhang of the roof.</summary>
            public Color color; /// <summary>Color of the roof.</summary>

            /// <summary>
            /// Constructor with type, thickness, and overhang parameters.
            /// </summary>
            /// <param name="type">The type of the roof.</param>
            /// <param name="thickness">Thickness of the roof.</param>
            /// <param name="overhang">Overhang of the roof.</param>
            public RoofConfig(RoofType type, float thickness, float overhang)
            {
                this.type = type;
                this.thickness = thickness;
                this.overhang = overhang;
            }

            /// <summary>
            /// Constructor with type, thickness, overhang, and color parameters.
            /// </summary>
            /// <param name="type">The type of the roof.</param>
            /// <param name="thickness">Thickness of the roof.</param>
            /// <param name="overhang">Overhang of the roof.</param>
            /// <param name="color">Color of the roof.</param>
            public RoofConfig(RoofType type, float thickness, float overhang, Color color)
            {
                this.type = type;
                this.thickness = thickness;
                this.overhang = overhang;
                this.color = color;
            }
        }

        /// <summary>
        /// Unsubscribes from events to prevent memory leaks when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnFileLoaded -= OnFileLoaded;
            GameEditorEvents.OnRequestButtonAction -= OnRequestButtonActionHandler;
        }

        /// <summary>
        /// Initializes the singleton instance of AppController.
        /// </summary>
        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Performs initialization tasks when the script instance is being loaded.
        /// </summary>
        void Start()
        {
#if !UNITY_EDITOR
            AlertPopup.ShowAlert("welcome", "Welcome!", "For any help or comment, shoot us a message at support.exoa.fr!", false);
#endif
            // Fetches all modules from the backend
            ModuleDataModels.GetAllModules(OnGetModules);

            // Subscribes to game editor events
            if (currentState != States.PlayMode)
            {
                GameEditorEvents.OnFileLoaded += OnFileLoaded;
            }
            GameEditorEvents.OnRequestButtonAction += OnRequestButtonActionHandler;

#if UNITY_ANDROID || UNITY_IOS
            RenderSettings.fog = false;
#endif
        }

        /// <summary>
        /// Handles button action requests from the game editor events.
        /// </summary>
        /// <param name="action">The action requested.</param>
        /// <param name="active">Is the action active?</param>
        private void OnRequestButtonActionHandler(GameEditorEvents.Action action, bool active)
        {
            if (action == GameEditorEvents.Action.ExportFBX)
            {
                string fileName = UISaving.instance.CurrentFileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (State == States.PreviewBuilding)
                    {
                        FBXExporter.ExportScene("Building_" + fileName, GameObject.Find("FinalBuilding"));
                    }
                    else
                    {
                        FBXExporter.ExportScene("Floor_" + fileName, gameObject);
                    }

                }
            }
        }

        /// <summary>
        /// Updates the camera focus or toggles the main canvas visibility based on input.
        /// </summary>
        void Update()
        {
            if (HDInputs.ResetCamera())
            {
                ReFocusCamera();
            }
#if UNITY_EDITOR
            if (BaseTouchInput.GetKeyWentDown(KeyCode.P) && BuildOptions.DEBUG_MODE)
                UnityEditor.EditorApplication.isPaused = true;
#endif
            if (BaseTouchInput.GetKeyWentDown(KeyCode.I) && BuildOptions.DEBUG_MODE)
            {
                Canvas mainCanvas = GameObject.FindObjectOfType<Canvas>();
                if (mainCanvas != null) mainCanvas.enabled = !mainCanvas.enabled;
            }

        }

        /// <summary>
        /// Handles the event triggered when a file is loaded, setting a delayed focus on the camera.
        /// </summary>
        /// <param name="fileType">The type of file that was loaded.</param>
        private void OnFileLoaded(FileType fileType)
        {
            if (fileType == FileType.FloorMapFile || fileType == FileType.BuildingRead || fileType == FileType.InteriorFile)
            {
                Invoke("DelayedFocus", .5f);
            }
        }

        /// <summary>
        /// Invokes the camera refocus with a delay.
        /// </summary>
        public void DelayedFocus()
        {
            ReFocusCamera();
        }

        /// <summary>
        /// Refocuses the camera on the current game object.
        /// </summary>
        public void ReFocusCamera()
        {
            CameraEvents.OnRequestObjectFocus?.Invoke(gameObject, true);
        }

        /* DEPRECATED
        /// <summary>
        /// Deprecated method to recenter the level.
        /// </summary>
        private void ReCenterLevel()
        {
            if (!recenterContainerOnLoad)
                return;

            Bounds b = globalContainer.GetBoundsRecursive();
            globalContainer.transform.position -= b.center.SetY(0);
            transform.position -= b.center.SetY(0);
        }*/

        /// <summary>
        /// Callback method for receiving the list of modules from the backend.
        /// </summary>
        /// <param name="list">The list of modules received from the backend.</param>
        private void OnGetModules(ModuleDataModels.ModuleList list)
        {
            //print("OnGetModules list:" + list.modules.Length);
            backendModuleList = list;
        }

        /// <summary>
        /// Retrieves a module by its prefab name.
        /// </summary>
        /// <param name="name">The prefab name of the module.</param>
        /// <returns>The module corresponding to the prefab name.</returns>
        public ModuleDataModels.Module GetModuleByPrefab(string name)
        {
            foreach (ModuleDataModels.Module p in backendModuleList.modules)
            {
                if (p.prefab == name || p.prefab + "(Clone)" == name)
                {
                    return p;
                }
            }
            return new ModuleDataModels.Module("", "");
        }

        /// <summary>
        /// Retrieves the current building settings.
        /// </summary>
        /// <returns>A DataModel.BuildingSettings object containing current settings.</returns>
        public DataModel.BuildingSettings GetBuildingSettings()
        {
            DataModel.BuildingSettings s = new DataModel.BuildingSettings();
            s.wallsHeight = wallsHeight;
            s.doorsHeight = doorsHeight;
            s.interiorWallThickness = interiorWallThickness;
            s.exteriorWallThickness = exteriorWallThickness;
            s.windowsThickness = windowsThickness;
            s.doorsThickness = doorsThickness;
            s.roofOverhang = roof.overhang;
            s.roofThickness = roof.thickness;
            s.roofType = (int)roof.type;
            return s;
        }

#if FLOORMAP_MODULE
        /// <summary>
        /// Sets the building settings from a provided DataModel.BuildingSettings object.
        /// Only active if the FLOORMAP_MODULE is defined.
        /// </summary>
        /// <param name="s">The building settings to apply.</param>
        public void SetFloorMapSettings(DataModel.BuildingSettings s)
        {
            wallsHeight = s.wallsHeight;
            doorsHeight = s.doorsHeight;
            interiorWallThickness = s.interiorWallThickness;
            exteriorWallThickness = s.exteriorWallThickness;
            windowsThickness = s.windowsThickness;
            doorsThickness = s.doorsThickness;
            roof.overhang = s.roofOverhang;
            roof.thickness = s.roofThickness;
            AppController.RoofType[] values = Enum.GetValues(typeof(AppController.RoofType)) as AppController.RoofType[];
            roof.type = values[Mathf.RoundToInt(s.roofType)];
        }
#else
        /// <summary>
        /// Placeholder for retrieving floor map settings when the FLOORMAP_MODULE is not defined.
        /// </summary>
        /// <returns>null since the module is not defined.</returns>
        public object GetFloorMapSettings()
        {
            return null;
        }

        /// <summary>
        /// Placeholder for setting floor map settings when the FLOORMAP_MODULE is not defined.
        /// </summary>
        /// <param name="s">The settings to apply (not used).</param>
        public void SetFloorMapSettings(object s)
        {
            // No implementation needed when FLOORMAP_MODULE is not defined
        }
#endif
    }
}
