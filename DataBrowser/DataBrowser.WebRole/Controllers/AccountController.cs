using System;
using System.Globalization;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Samples.ServiceHosting.AspProviders;
using Ogdi.InteractiveSdk.Mvc.Models;

namespace MvcColudWeb.Controllers
{

    [HandleError]
    public class AccountController : Controller
    {

        // This constructor is used by the MVC framework to instantiate the controller using
        // the default forms authentication and membership providers.

        public AccountController()
            : this(null, null, null)
        {
        }

        // This constructor is not used by the MVC framework but is instead provided for ease
        // of unit testing this type. See the comments at the end of this file for more
        // information.
        public AccountController(IFormsAuthentication formsAuth, IMembershipService service, IRoleService roleService)
        {
            FormsAuth = formsAuth ?? new FormsAuthenticationService();
            MembershipService = service ?? new AccountMembershipService();
            RoleService = roleService ?? new AccountRoleService();
        }

        public IFormsAuthentication FormsAuth
        {
            get;
            private set;
        }

        public IMembershipService MembershipService
        {
            get;
            private set;
        }

        public IRoleService RoleService
        {
            get;
            private set;
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Needs to take same parameter type as Controller.Redirect()")]
        public ActionResult LogOn(string userName, string password, bool rememberMe, string returnUrl)
        {

            if (!ValidateLogOn(userName, password))
            {
                return View();
            }

            FormsAuth.SignIn(userName, rememberMe);
            if (MembershipService.GetType().IsAssignableFrom(typeof(AccountMembershipService)))
            {
                var userData = 
                    ((AccountMembershipService) MembershipService).GetUserAdditionalData(userName);
            }


            if(RoleService.IsUserInRole(userName, "Administrator"))
            {
                Session["UserRole"] = "Administrator";
            }

            if (!String.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LogOff()
        {
            Session["UserRole"] = null;
            FormsAuth.SignOut();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {

            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Register(
            string userName, string email, 
            string password, string confirmPassword, 
            string firstName, string lastName)
        {
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;

            if (ValidateRegistration(userName, email, password, confirmPassword))
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(userName, password, email);
                RoleService.AddUserToRole(userName, "Public");
                if (MembershipService.GetType().IsAssignableFrom(typeof(AccountMembershipService)))
                {
                        ((AccountMembershipService)MembershipService).SetUserAdditionalData(userName, 
                            new TableStorageMembershipProvider.AdditionalUserData(firstName, lastName));
                }

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuth.SignIn(userName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("_FORM", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [Authorize]
        public ActionResult UpdateUserInfo()
        {
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;

            var user = MembershipService.GetUser(User.Identity.Name);

            string firstName = string.Empty;
            string lastName = string.Empty;

            if (MembershipService.GetType().IsAssignableFrom(typeof(AccountMembershipService)))
            {
                var userData = ((AccountMembershipService)MembershipService)
                    .GetUserAdditionalData(User.Identity.Name);
                firstName = userData.FirstName;
                lastName = userData.LastName;
            }

            ViewData["UserName"] = User.Identity.Name;
            ViewData["Email"] = user.Email;
            ViewData["FirstName"] = firstName;
            ViewData["LastName"] = lastName;

            return View();
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exceptions result in password not being changed.")]
        public ActionResult UpdateUserInfo(string currentPassword, string newPassword, 
            string confirmPassword, string email, string firstName, string lastName)
        {

            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;

            if (!ValidateUserInfo(currentPassword, newPassword, confirmPassword, email))
            {
                return View();
            }

            try
            {
                var changePasswordResult =
                    MembershipService.ChangePassword(User.Identity.Name, currentPassword, newPassword);
                MembershipService.UpdateUserEMial(User.Identity.Name, email);
                if (MembershipService.GetType().IsAssignableFrom(typeof(AccountMembershipService)))
                {
                    var data = new TableStorageMembershipProvider.AdditionalUserData(firstName, lastName);
                    ((AccountMembershipService)MembershipService).SetUserAdditionalData(User.Identity.Name, data);
                }
                if (changePasswordResult)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("_FORM", "The current password is incorrect or the new password is invalid.");
                return View();
            }
            catch
            {
                ModelState.AddModelError("_FORM", "The current password is incorrect or the new password is invalid.");
                return View();
            }
        }        

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public ActionResult ManageRoles()
        {
            // Hack: Create three dummy roles if no roles are currently in the role system
            // I am using this to avoid having to write a setup script.
            string[] roles = RoleService.GetAllRoles();

            return View(new ManageRolesModel());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ManageRoles(string UserName, string RoleName, string Operation)
        {
            try
            {
                string userName = UserName;
                string roleName = RoleName;

                if (!ValidateManageRole(UserName, RoleName))
                {
                    return View(new ManageRolesModel());
                }

                if(Operation == "Add")
                {
                    if (RoleService.IsUserInRole(UserName, RoleName))
                    {
                        ModelState.AddModelError("UserName", "Role already assigned to user");
                        return View(new ManageRolesModel());
                    }
                    RoleService.AddUserToRole(UserName, RoleName);
                    
                }
                else if(Operation == "Remove")
                {
                    if (!RoleService.IsUserInRole(UserName, RoleName))
                    {
                        ModelState.AddModelError("UserName", "Role not assigned to user");
                        return View(new ManageRolesModel());
                    }
                    RoleService.RemoveUserFromRole(UserName, RoleName);
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError("_FORM", "The input data is invalid.");
                return View(new ManageRolesModel(UserName));
            }
        }

        private bool ValidateManageRole(string UserName, string RoleName)
        {
            if(!MembershipService.UserExists(UserName))
            {
                ModelState.AddModelError("UserName", "There is no such user");
            }
            return ModelState.IsValid;
        }


        public ActionResult Initialize()
        {
            string[] roles = RoleService.GetAllRoles();

            if (roles.Length == 0)
            {
                RoleService.CreateRole("Public");
                RoleService.CreateRole("Agency");
                RoleService.CreateRole("Administrator");

                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser("admin", "12345", "admin@fakecompany.com");
                RoleService.AddUserToRole("admin", "Administrator");
                FormsAuth.SignIn("admin", false /* createPersistentCookie */);
            }
            return RedirectToAction("Index", "Home");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity is WindowsIdentity)
            {
                throw new InvalidOperationException("Windows authentication is not supported.");
            }
        }        

        #region Validation Methods

        private bool ValidateUserInfo(string currentPassword, string newPassword, string confirmPassword, string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "You must specify an email address.");
            }
            if (String.IsNullOrEmpty(currentPassword))
            {
                ModelState.AddModelError("currentPassword", "You must specify a current password.");
            }
            if (newPassword == null || newPassword.Length < MembershipService.MinPasswordLength)
            {
                ModelState.AddModelError("newPassword",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a new password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }
            if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }

            return ModelState.IsValid;
        }

        private bool ValidateLogOn(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }
            if (String.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "You must specify a password.");
            }
            if (!MembershipService.ValidateUser(userName, password))
            {
                ModelState.AddModelError("_FORM", "The login or password provided is incorrect.");
            }

            return ModelState.IsValid;
        }

        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword)
        {
            if (String.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }
            if (String.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "You must specify an email address.");
            }
            if (password == null || password.Length < MembershipService.MinPasswordLength)
            {
                ModelState.AddModelError("password",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }
            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }
            return ModelState.IsValid;
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }

    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthentication
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
        bool UserExists(string userName);
        MembershipUser GetUser(string userName);
        void UpdateUserEMial(string userName, string email);
    }

    public class AccountMembershipService : IMembershipService
    {
        private MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            return _provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
            return currentUser.ChangePassword(oldPassword, newPassword);
        }

        public bool UserExists(string userName)
        {
            return _provider.GetUser(userName, false) != null;
        }

        public MembershipUser GetUser(string userName)
        {
            return _provider.GetUser(userName, false);
        }

        public void UpdateUserEMial(string userName, string email)
        {
            var user = _provider.GetUser(userName, false);
            user.Email = email;
            _provider.UpdateUser(user);
        }

        public TableStorageMembershipProvider.AdditionalUserData GetUserAdditionalData(string userName)
        {
            if(_provider.GetType().IsAssignableFrom(typeof(TableStorageMembershipProvider)))
            {
                return ((TableStorageMembershipProvider) _provider).GetUserAdditionalData(userName);
            }
            return null;
        }

        public void SetUserAdditionalData(string userName, 
            TableStorageMembershipProvider.AdditionalUserData data)
        {
            if(_provider.GetType().IsAssignableFrom(typeof(TableStorageMembershipProvider)))
            {
                ((TableStorageMembershipProvider)_provider).SetUserAdditionalData(userName, data);
            }
        }
        
    }

    public interface IRoleService
    {
        bool IsUserInRole(string userName, string roleName);
        void AddUserToRole(string userName, string roleName);
        void RemoveUserFromRole(string userName, string roleName);
        void CreateRole(string roleName);
        void DeleteRole(string roleName);
        string[] GetAllRoles();
    }

    public class AccountRoleService : IRoleService
    {
        private readonly RoleProvider roleProvider;

        public AccountRoleService()
            : this(null)
        {
        }

        public AccountRoleService(RoleProvider provider)
        {
            this.roleProvider = provider ?? (Roles.Enabled ? Roles.Provider : null);
        }

        public void AddUserToRole(string userName, string roleName)
        {
            if (this.roleProvider != null)
            {
                this.roleProvider.AddUsersToRoles(new string[] { userName }, new string[] { roleName });
            }
        }

        public void RemoveUserFromRole(string userName, string roleName)
        {
            if (this.roleProvider != null)
            {
                this.roleProvider.RemoveUsersFromRoles(new string[] { userName }, new string[] { roleName });
            }
        }


        public bool IsUserInRole(string userName, string roleName)
        {
            if (this.roleProvider != null)
            {
                return this.roleProvider.IsUserInRole(userName, roleName);
            }

            return false;
        }

        public void CreateRole(string roleName)
        {
            if (this.roleProvider != null)
            {
                this.roleProvider.CreateRole(roleName);
            }
        }

        public void DeleteRole(string roleName)
        {
            if (this.roleProvider != null)
            {
                this.roleProvider.DeleteRole(roleName, false);
            }
        }

        public string[] GetAllRoles()
        {
            if (this.roleProvider != null)
            {
                return this.roleProvider.GetAllRoles();
            }

            return new string[0];
        }
    }
}
