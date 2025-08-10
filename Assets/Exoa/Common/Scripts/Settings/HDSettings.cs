
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// A class that contains various constant settings used in the Home Designer application for handling 
    /// file paths and related resources.
    /// </summary>
    public class HDSettings
    {
        public const string EXT_INTERIOR_FOLDER = "Interior";
        public const string EXT_FLOORMAP_FOLDER = "FloorMaps";
        public const string EXT_THUMBNAIL_FOLDER = "Thumbnails";
        public const string EXT_EXPORT_FOLDER = "Exports";

        public const string EMBEDDED_FLOORMAP_FOLDER = "EmbeddedFloorMaps";
        public const string EMBEDDED_THUMBNAIL_FOLDER = "EmbeddedThumbnails";

        //public const string FLOORMAP_INTERNAL_FOLDER = "/Exoa/HomeDesigner/Samples/SampleData_Kitchen/Resources/EmbeddedFloorMaps";
        public const string MODULE_THUMBNAIL_FOLDER = "InteriorModuleThumbnails/";

        public const string FLOOR_MATERIALS_FOLDER = "Floor/";
        public const string WALL_MATERIALS_FOLDER = "Wall/";
        public const string EXTERIOR_WALL_MATERIALS_FOLDER = "ExteriorWall/";
        public const string ROOF_MATERIALS_FOLDER = "Roof/";
        public const string CEILING_MATERIALS_FOLDER = "Ceiling/";
        public const string OUTSIDE_MATERIALS_FOLDER = "Outside/";
        public const string MODULES_FOLDER = "InteriorModules/";

        public const string MODULES_JSON = "interior_modules";
        public const string CATEGORIES_JSON = "interior_categories";

#if UNITY_PIPELINE_HDRP || UNITY_PIPELINE_URP
        /// <summary>
        /// The background color used for thumbnails in HDRP or URP pipelines.
        /// </summary>
        public static Color THUMBNAIL_BACKGROUND = new Color(1.0f, 1.0f, 1.0f, 1.0f);
#else
        /// <summary>
        /// The background color used for thumbnails in other pipelines.
        /// </summary>
        public static Color THUMBNAIL_BACKGROUND = new Color(1.0f, 1.0f, 1.0f, 0.0f);
#endif

        public const string INTERIOR_DESIGNER_THUMBNAIL_PREFIX = "Interior_";
        public const string FLOORMAP_DESIGNER_THUMBNAIL_PREFIX = "Floormap_";
        public const string BUILDING_THUMBNAIL_PREFIX = "Building_";

    }
}
