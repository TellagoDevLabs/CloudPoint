using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPSM.Common.Security
{
    public class SecurityHelper
    {
        public static SPRoleDefinition CreateRoleDefinition(SPWeb web, string name, string description, SPBasePermissions permissions, string roleDefinitionToCopy)
        {
            SPRoleDefinition roleDefinitionBase = GetRoleDefinition(web, roleDefinitionToCopy);
            SPRoleDefinition roleDefinition = GetRoleDefinition(web, name);

            if (roleDefinition != null)
                return null;

            if (roleDefinitionBase != null)
                roleDefinition = new SPRoleDefinition(roleDefinitionBase);
            else
                roleDefinition = new SPRoleDefinition();

            roleDefinition.BasePermissions = roleDefinition.BasePermissions | permissions;
            roleDefinition.Name = name;
            roleDefinition.Description = description;

            web.AllowUnsafeUpdates = true;
            web.RoleDefinitions.Add(roleDefinition);
            web.Update();
            web.AllowUnsafeUpdates = false;

            return roleDefinition;
        }

        public static SPGroup AddGroupToWeb(SPWeb web, string name, string description, string role, SPMember owner)
        {
            web.AllowUnsafeUpdates = true;

            if (!web.IsRootWeb)
                web.BreakRoleInheritance(false);

            SPGroup oGroup = GetSiteGroup(web, name);
            if (oGroup == null)
            {
                //--------if description is null the search crawl on the site collection breaks with error:
                //The SharePoint item being crawled returned an error when requesting data from the web service. ( Error from SharePoint site: Data is Null. This method or property cannot be called on Null values. )
                if (String.IsNullOrEmpty(description))
                    description = name;                
                //--------
                web.SiteGroups.Add(name, owner, null, description);
                oGroup = GetSiteGroup(web, name);
            }

            if (!String.IsNullOrEmpty(role))
            {
                SPRoleAssignment oRoleAssignment = new SPRoleAssignment(oGroup);
                oRoleAssignment.RoleDefinitionBindings.Add(web.RoleDefinitions[role]);
                web.RoleAssignments.Add(oRoleAssignment);
            }

            web.Update();
            web.AllowUnsafeUpdates = false;

            return oGroup;
        }

        public static void AssignPermissionToWeb(SPWeb web, SPPrincipal userOrGroup, string roleDefinitionName)
        {
            if (!web.IsRootWeb)
            {
                web.BreakRoleInheritance(true);
                SPRoleAssignment roleAssignment = new SPRoleAssignment(userOrGroup);
                SPRoleDefinition roleDefinition = web.RoleDefinitions[roleDefinitionName];
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                web.RoleAssignments.Add(roleAssignment);
            }
        }

        public static void AssignPermissionToList(SPList list, SPPrincipal userOrGroup, string roleDefinitionName)
        {
            list.BreakRoleInheritance(false);
            SPRoleAssignment roleAssignment = new SPRoleAssignment(userOrGroup);
            SPRoleDefinition roleDefinition = list.ParentWeb.RoleDefinitions[roleDefinitionName];
            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
            list.RoleAssignments.Add(roleAssignment);
        }

        public static void AssignPermissionToItem(SPListItem item, SPPrincipal userOrGroup, string roleDefinitionName)
        {
            item.BreakRoleInheritance(false);
            SPRoleAssignment roleAssignment = new SPRoleAssignment(userOrGroup);
            SPRoleDefinition roleDefinition = item.ParentList.ParentWeb.RoleDefinitions[roleDefinitionName];
            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
            item.RoleAssignments.Add(roleAssignment);
        }

        public static void RemovePermissionsFromItem(SPListItem item, SPPrincipal userOrGroup)
        {
            item.RoleAssignments.Remove(userOrGroup);
        }

        public static SPGroup GetSiteGroup(SPWeb web, string name)
        {
            foreach (SPGroup group in web.SiteGroups)
                if (group.Name.ToLower() == name.ToLower())
                    return group;

            return null;
        }

        public static SPUser GetUser(SPWeb currentWeb, string userLogin)
        {
            SPUser user = null;

            Elevation.OnWeb(currentWeb, web =>
            {
                foreach (SPUser u in web.AllUsers)
                {
                    if (u.LoginName == userLogin)
                        user = web.AllUsers[userLogin];
                }

                if (user == null)
                    user = web.CurrentUser;
            });

            return user;
        }

        public static bool ValidateUser(SPWeb web, string name)
        {
            bool result = false;

            SPFieldUserValue user = new SPFieldUserValue(web, name);

            if (user != null)
            {
                if (user.User != null)
                    result = user.User.ID.Equals(web.CurrentUser.ID);
                else
                    result = web.IsCurrentUserMemberOfGroup(user.LookupId);
            }

            return result;
        }

        private static SPRoleDefinition GetRoleDefinition(SPWeb web, string name)
        {
            SPRoleDefinition retVal = null;

            foreach (SPRoleDefinition rd in web.RoleDefinitions)
            {
                if (rd.Name == name)
                {
                    retVal = rd;
                    break;
                }
            }

            return retVal;
        }
    }
}
