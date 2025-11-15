using Microsoft.AspNetCore.Components;
using Group5.Data;
using Group5.Services.Interfaces;
using Group5.Shared;

namespace Group5.Core.Base
{
    /// <summary>
    /// Base Admin Page - Demonstrates INHERITANCE
    /// Inherits from BasePage (IS-A relationship)
    /// UML: GENERALIZATION - BaseAdminPage IS-A BasePage
    /// Provides common functionality for all admin pages
    /// Demonstrates ABSTRACTION - common admin behavior
    /// </summary>
    public abstract class BaseAdminPage : BasePage
    {
        // Services - Demonstrates ASSOCIATION (uses-a relationship)
        [Inject] protected AppDbContext Db { get; set; } = null!;
        [Inject] protected IInventoryService InventoryService { get; set; } = null!;

        // Common admin properties - Demonstrates ENCAPSULATION
        protected string message = string.Empty;

        /// <summary>
        /// Override OnInitialized to check admin role
        /// Demonstrates POLYMORPHISM - method overriding
        /// </summary>
        protected override void OnInitialized()
        {
            // Check if user is logged in
            if (string.IsNullOrWhiteSpace(UserSession.Email))
            {
                Console.WriteLine($"⚠️ {GetType().Name}: No session found; redirecting");
                Navigation.NavigateTo("/", forceLoad: true);
                return;
            }

            // Admin-specific initialization
            displayedUsername = UserSession.Username;
            displayedEmail = UserSession.Email;
            
            // Call template method
            OnAdminPageInitialized();
        }

        /// <summary>
        /// Template method for admin pages
        /// Child classes can override for specific initialization
        /// Demonstrates POLYMORPHISM
        /// </summary>
        protected virtual void OnAdminPageInitialized()
        {
            Console.WriteLine($"✅ {GetType().Name} loaded for admin: {displayedUsername}");
        }

        /// <summary>
        /// Display temporary message
        /// Common utility for all admin pages
        /// </summary>
        protected async Task ShowMessageAsync(string msg, int durationMs = 3000)
        {
            message = msg;
            StateHasChanged();
            await Task.Delay(durationMs);
            message = string.Empty;
            StateHasChanged();
        }
    }
}

