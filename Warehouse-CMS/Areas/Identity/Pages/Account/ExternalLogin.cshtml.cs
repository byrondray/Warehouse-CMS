using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            IEmployeeRepository employeeRepository,
            IEmployeeRoleRepository employeeRoleRepository
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _employeeRepository = employeeRepository;
            _employeeRoleRepository = employeeRoleRepository;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ProviderDisplayName { get; set; } = string.Empty;

        public string ReturnUrl { get; set; } = string.Empty;

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> AvailableRoles { get; set; } =
            Enumerable.Empty<SelectListItem>();

        // Only non-privileged roles may be self-assigned during external-login registration;
        // Admin/Manager are granted by an existing admin through the Roles UI.
        private async Task<IEnumerable<SelectListItem>> GetSelfAssignableRolesAsync() =>
            (await _employeeRoleRepository.GetAllAsync())
                .Where(r => !RoleConstants.IsPrivileged(r.Role))
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Role })
                .ToList();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Full Name")]
            public string Name { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Start Date")]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required]
            [Display(Name = "Employee Role")]
            public int EmployeeRoleId { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page(
                "./ExternalLogin",
                pageHandler: "Callback",
                values: new { returnUrl }
            );
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                redirectUrl
            );
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(
            string returnUrl = null,
            string remoteError = null
        )
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true
            );
            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "{Name} logged in with {LoginProvider} provider.",
                    info.Principal.Identity.Name,
                    info.LoginProvider
                );
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                AvailableRoles = await GetSelfAssignableRolesAsync();

                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            _logger.LogInformation(
                "OnPostConfirmationAsync called with returnUrl: {ReturnUrl}",
                returnUrl
            );

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning(
                    "External login info is null during confirmation. User email from form: {Email}",
                    Input?.Email
                );

                // Check if user already exists with this email
                if (!string.IsNullOrEmpty(Input?.Email))
                {
                    var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                    if (existingUser != null)
                    {
                        _logger.LogWarning(
                            "User with email {Email} already exists but trying to register with external provider",
                            Input.Email
                        );
                        ErrorMessage =
                            "An account with this email already exists. Please sign in with your password instead, or contact support to link your Google account.";
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }
                }

                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            _logger.LogInformation(
                "External login info found. Provider: {Provider}, Email: {Email}",
                info.LoginProvider,
                info.Principal.FindFirstValue(ClaimTypes.Email)
            );

            if (ModelState.IsValid)
            {
                // Double-check for existing user before creating new account
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning(
                        "Attempted to create account for existing user {Email} via external provider {Provider}",
                        Input.Email,
                        info.LoginProvider
                    );

                    // Do NOT sign the existing account in here: the external identity
                    // (info.ProviderKey) has not been verified as belonging to this account.
                    // Signing in based only on a form-supplied email would allow anyone who
                    // completed any Google OAuth to take over an account by typing its email.
                    // Account linking must be done from an authenticated session, and a genuine
                    // provider match is already handled by ExternalLoginSignInAsync in the callback.
                    ModelState.AddModelError(
                        string.Empty,
                        $"An account with the email address {Input.Email} already exists. "
                            + "Please sign in with your existing credentials first, then you can link your Google account in your profile settings."
                    );

                    AvailableRoles = await GetSelfAssignableRolesAsync();

                    ProviderDisplayName = info.ProviderDisplayName;
                    ReturnUrl = returnUrl;
                    return Page();
                }

                // Never trust the submitted role id: reject any attempt to self-assign a
                // privileged role, even one crafted directly against the POST.
                var requestedRole = await _employeeRoleRepository.GetByIdAsync(Input.EmployeeRoleId);
                if (requestedRole == null || RoleConstants.IsPrivileged(requestedRole.Role))
                {
                    ModelState.AddModelError(
                        "Input.EmployeeRoleId",
                        "Please select a valid role. Administrative roles are assigned by a manager."
                    );

                    AvailableRoles = await GetSelfAssignableRolesAsync();
                    ProviderDisplayName = info.ProviderDisplayName;
                    ReturnUrl = returnUrl;
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation(
                            "User created an account using {Name} provider.",
                            info.LoginProvider
                        );

                        var employee = new Employee
                        {
                            Name = Input.Name,
                            StartDate = DateTime.SpecifyKind(Input.StartDate, DateTimeKind.Utc),
                            EmployeeRoleId = Input.EmployeeRoleId,
                            UserId = user.Id,
                        };

                        await _employeeRepository.AddAsync(employee);

                        // requestedRole was validated above: non-null and non-privileged.
                        await _userManager.AddToRoleAsync(user, requestedRole!.Role);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new
                            {
                                area = "Identity",
                                userId = userId,
                                code = code,
                                returnUrl = returnUrl,
                            },
                            protocol: Request.Scheme
                        );

                        await _emailSender.SendEmailAsync(
                            Input.Email,
                            "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
                        );

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage(
                                "./RegisterConfirmation",
                                new { Input.Email, returnUrl = returnUrl }
                            );
                        }

                        await _signInManager.SignInAsync(
                            user,
                            isPersistent: false,
                            info.LoginProvider
                        );
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            AvailableRoles = await GetSelfAssignableRolesAsync();

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Can't create an instance of '{nameof(IdentityUser)}'. "
                        + $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively "
                        + $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml"
                );
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException(
                    "The default UI requires a user store with email support."
                );
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
