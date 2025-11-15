using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Group5.Shared;

namespace Group5.Core.Base
{
    /// <summary>
    /// Base Page Component - Demonstrates ABSTRACTION and INHERITANCE
    /// Provides common functionality for all pages
    /// UML: This is the BASE CLASS in a GENERALIZATION hierarchy
    /// Child classes: BaseToolroomPage, BaseAdminPage, BaseProfessorPage
    /// Demonstrates ENCAPSULATION with protected members
    /// </summary>
    public abstract class BasePage : ComponentBase
    {
        // Dependency Injection - Services injected automatically
        [Inject] protected NavigationManager Navigation { get; set; } = null!;
        [Inject] protected IJSRuntime JS { get; set; } = null!;

        // Common properties for all pages - Demonstrates ENCAPSULATION
        protected string displayedUsername = string.Empty;
        protected string displayedEmail = string.Empty;
        protected bool isLoggingOut = false;
        protected string logoutButtonText = "Log Out";

        /// <summary>
        /// Initialize page - VIRTUAL method (can be overridden)
        /// Demonstrates POLYMORPHISM - child classes can override
        /// </summary>
        protected override void OnInitialized()
        {
            if (string.IsNullOrWhiteSpace(UserSession.Email))
            {
                Console.WriteLine($"⚠️ {GetType().Name}: No session found; redirecting to login");
                Navigation.NavigateTo("/", forceLoad: true);
                return;
            }

            displayedUsername = UserSession.Username;
            displayedEmail = UserSession.Email;
            
            // Call template method
            OnPageInitialized();
        }

        /// <summary>
        /// Template Method Pattern - child classes override this
        /// Demonstrates POLYMORPHISM
        /// </summary>
        protected virtual void OnPageInitialized()
        {
            Console.WriteLine($"✅ {GetType().Name} loaded for: {displayedUsername}");
        }

        /// <summary>
        /// Common logout functionality - Demonstrates CODE REUSE through inheritance
        /// </summary>
        protected async Task LogoutAsync()
        {
            isLoggingOut = true;
            logoutButtonText = "Logging out...";
            StateHasChanged();

            try
            {
                Console.WriteLine($"🚪 {GetType().Name}: LogoutAsync invoked");

                UserSession.Email = string.Empty;
                UserSession.Username = string.Empty;
                UserSession.Role = string.Empty;

                try
                {
                    await JS.InvokeVoidAsync("console.log", "🚪 Logout - clearing session");
                }
                catch { }

                Navigation.NavigateTo("/", forceLoad: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Logout error: {ex.Message}");
            }
            finally
            {
                isLoggingOut = false;
                logoutButtonText = "Log Out";
                StateHasChanged();
            }
        }

        /// <summary>
        /// Get short department name - utility method
        /// </summary>
        protected string GetDepartmentShortName(string department)
        {
            return department switch
            {
                "CIVIL ENGINEERING TOOLROOM" => "CE",
                "ELECTRONICS ENGINEERING TOOLROOM" => "ECE",
                "ELECTRICAL ENGINEERING TOOLROOM" => "EE",
                "CHEMISTRY LABORATORY TOOLROOM" => "CHEM",
                "PHYSICS LABORATORY TOOLROOM" => "PHYSICS",
                _ => department.Substring(0, Math.Min(6, department.Length))
            };
        }
    }
}

