using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;

namespace Tellago.SP.Media.Features.MediaWebElements
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("16d79454-0dd4-4186-bd87-1bd52b0f07f8")]
    public class MediaWebElementsEventReceiver : SPFeatureReceiver
    {
        private const string MediaAssetsLibName = "MediaAssetsLib";

        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            if (MustCreateMediaAssetsLib(properties))
            {
                SPWeb web = properties.Feature.Parent as SPWeb;
                CreateMediaLibrary(web);
            }
        }

        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            if (MustCreateMediaAssetsLib(properties))
            {
                //delete media library
                SPWeb web = properties.Feature.Parent as SPWeb;
                var lib = web.Lists.TryGetList(MediaAssetsLibName);
                if (lib != null)
                {
                    lib.Delete();
                } 
            }
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}

        private static bool MustCreateMediaAssetsLib(SPFeatureReceiverProperties properties)
        {
            bool createMediaAssetsLib = false;
            SPFeaturePropertyCollection featureProps = properties.Feature.Properties;
            if (featureProps != null && featureProps.Count > 0)
            {
                try
                {
                    SPFeatureProperty createMediaAssetsLibProp = featureProps["CreateMediaAssetsLib"];
                    if (createMediaAssetsLibProp != null)
                    {
                        bool.TryParse(createMediaAssetsLibProp.Value, out createMediaAssetsLib);
                    }
                }
                catch
                {
                    //consider a FALSE if the property is not present..
                }
            }
            return createMediaAssetsLib;
        }

        private void CreateMediaLibrary(SPWeb web)
        {
            var lib = web.Lists.TryGetList(MediaAssetsLibName);
            if (lib == null)
            {
                SPListTemplate listTemplate = web.ListTemplates["Asset Library"];
                Guid newListId = web.Lists.Add(MediaAssetsLibName, "Media Assets Storage", listTemplate);
                SPDocumentLibrary newLibrary = web.Lists[newListId] as SPDocumentLibrary;
                newLibrary.OnQuickLaunch = false;
                newLibrary.EnableVersioning = true;
                newLibrary.NoCrawl = true;
                newLibrary.Update();
            }
        }


    }
}
