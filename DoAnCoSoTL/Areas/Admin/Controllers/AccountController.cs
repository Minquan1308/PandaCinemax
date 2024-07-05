//using Azure.Core;
//using DoAnCoSoTL.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
//using System.Security.Claims;
//using System.Security.Policy;

//public class AccountController : Controller
//{
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly IEmailSender _emailSender;

//    public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
//    {
//        _userManager = userManager;
//        _emailSender = emailSender;
//    }

//    public async Task<IActionResult> ExternalLoginCallback()
//    {
//        var info = await _signInManager.GetExternalLoginInfoAsync();
//        if (info == null)
//        {
//            return RedirectToAction(nameof(Login));
//        }

//        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
//        if (result.Succeeded)
//        {
//            // User logged in with an external login provider
//            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
//            if (!user.EmailConfirmed)
//            {
//                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
//                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
//            }

//            return RedirectToAction("Index", "Home");
//        }

//        // If the user does not have an account, create one
//        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
//        var newUser = new ApplicationUser { UserName = email, Email = email };
//        var identityResult = await _userManager.CreateAsync(newUser);
//        if (identityResult.Succeeded)
//        {
//            identityResult = await _userManager.AddLoginAsync(newUser, info);
//            if (identityResult.Succeeded)
//            {
//                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
//                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, token = token }, Request.Scheme);
//                await _emailSender.SendEmailAsync(newUser.Email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

//                await _signInManager.SignInAsync(newUser, isPersistent: false);
//                return RedirectToAction("Index", "Home");
//            }
//        }

//        return RedirectToAction(nameof(Login));
//    }

//    [HttpGet]
//    public async Task<IActionResult> ConfirmEmail(string userId, string token)
//    {
//        if (userId == null || token == null)
//        {
//            return RedirectToAction("Index", "Home");
//        }

//        var user = await _userManager.FindByIdAsync(userId);
//        if (user == null)
//        {
//            return RedirectToAction("Index", "Home");
//        }

//        var result = await _userManager.ConfirmEmailAsync(user, token);
//        if (result.Succeeded)
//        {
//            return View("ConfirmEmail");
//        }

//        return RedirectToAction("Index", "Home");
//    }
//}
