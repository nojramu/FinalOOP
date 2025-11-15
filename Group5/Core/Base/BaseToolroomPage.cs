using Microsoft.AspNetCore.Components;
using Group5.Data;
using Group5.Models;
using Group5.Services.Interfaces;
using Group5.Shared;

namespace Group5.Core.Base
{
    /// <summary>
    /// Base Toolroom Page - Demonstrates INHERITANCE
    /// Inherits from BasePage (IS-A relationship)
    /// UML: GENERALIZATION - BaseToolroomPage IS-A BasePage
    /// Provides common functionality for all toolroom pages (CE, ECE, EE, CHEM, PHYSICS)
    /// Demonstrates ABSTRACTION - defines template for toolroom behavior
    /// </summary>
    public abstract class BaseToolroomPage : BasePage
    {
        // Services - Demonstrates ASSOCIATION (uses-a relationship)
        [Inject] protected AppDbContext Db { get; set; } = null!;
        [Inject] protected IInventoryService InventoryService { get; set; } = null!;

        // Modal state - Demonstrates ENCAPSULATION
        protected bool showModal = false;
        protected ToolroomItem? selectedItem = null;
        protected int selectedQuantity = 1;
        protected string addToCartMessage = string.Empty;

        // Items list - will be loaded from database
        protected List<ToolroomItem> items = new();

        /// <summary>
        /// Abstract property - child classes MUST implement
        /// Demonstrates ABSTRACTION - forces children to provide department name
        /// </summary>
        protected abstract string DepartmentName { get; }

        /// <summary>
        /// Initialize toolroom page - calls base and loads items
        /// Demonstrates method OVERRIDING (polymorphism)
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(UserSession.Email))
            {
                Console.WriteLine($"⚠️ {DepartmentName}: No session found; redirecting");
                Navigation.NavigateTo("/", forceLoad: true);
                return;
            }

            displayedUsername = UserSession.Username;
            displayedEmail = UserSession.Email;
            
            // Load items from database
            await LoadItemsAsync();
            
            Console.WriteLine($"✅ {DepartmentName} loaded for: {displayedUsername}");
        }

        /// <summary>
        /// Load items from database - common for all toolrooms
        /// </summary>
        protected virtual async Task LoadItemsAsync()
        {
            try
            {
                var inventoryItems = await InventoryService.GetDepartmentInventoryAsync(DepartmentName);
                items = inventoryItems
                    .Where(inv => !(DepartmentName == "CHEMISTRY LABORATORY TOOLROOM" && inv.ItemName == "Volumetric Flask")) // Exclude Volumetric Flask from Chemistry
                    .Select(inv => new ToolroomItem
                    {
                        Name = inv.ItemName,
                        Available = inv.AvailableQuantity,
                        MaxPerStudent = inv.MaxPerStudent,
                        Description = inv.Description,
                        Department = DepartmentName // Set department for image path generation
                    }).ToList();
                
                Console.WriteLine($"✅ Loaded {items.Count} items from {DepartmentName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading inventory: {ex.Message}");
            }
        }

        /// <summary>
        /// Open modal for item selection
        /// </summary>
        protected void OpenModal(ToolroomItem item)
        {
            selectedItem = item;
            selectedQuantity = 1;
            showModal = true;
            addToCartMessage = string.Empty;
            StateHasChanged();
        }

        /// <summary>
        /// Close modal
        /// </summary>
        protected void CloseModal()
        {
            showModal = false;
            selectedItem = null;
            selectedQuantity = 1;
            addToCartMessage = string.Empty;
            StateHasChanged();
        }

        /// <summary>
        /// Increase quantity
        /// </summary>
        protected void IncreaseQuantity()
        {
            if (selectedItem != null && 
                selectedQuantity < selectedItem.MaxPerStudent && 
                selectedQuantity < selectedItem.Available)
            {
                selectedQuantity++;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Decrease quantity
        /// </summary>
        protected void DecreaseQuantity()
        {
            if (selectedQuantity > 1)
            {
                selectedQuantity--;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Add to cart with validation
        /// </summary>
        protected async Task AddToCart()
        {
            if (selectedItem == null || string.IsNullOrWhiteSpace(UserSession.Email))
                return;

            try
            {
                var result = await InventoryService.AddToCartAsync(
                    UserSession.Email,
                    DepartmentName,
                    selectedItem.Name,
                    selectedQuantity
                );

                if (result.Success)
                {
                    addToCartMessage = result.Message;
                    await LoadItemsAsync(); // Refresh availability
                    await Task.Delay(1000);
                    CloseModal();
                }
                else
                {
                    addToCartMessage = result.Message;
                }
                
                StateHasChanged();
            }
            catch (Exception ex)
            {
                addToCartMessage = $"Error: {ex.Message}";
                StateHasChanged();
            }
        }

        /// <summary>
        /// Inner class for toolroom items - Demonstrates COMPOSITION
        /// ToolroomItem is part of BaseToolroomPage
        /// </summary>
        public class ToolroomItem
        {
            public string Name { get; set; } = string.Empty;
            public int Available { get; set; }
            public int MaxPerStudent { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            
            /// <summary>
            /// Get the image path based on department and item name
            /// </summary>
            public string GetImagePath()
            {
                // Map department names to folder prefixes
                var departmentPrefix = Department switch
                {
                    "CIVIL ENGINEERING TOOLROOM" => "CE",
                    "ELECTRONICS ENGINEERING TOOLROOM" => "ECE",
                    "CHEMISTRY LABORATORY TOOLROOM" => "CHEM",
                    "ELECTRICAL ENGINEERING TOOLROOM" => "EE",
                    "PHYSICS LABORATORY TOOLROOM" => "Physics",
                    _ => ""
                };
                
                if (string.IsNullOrEmpty(departmentPrefix))
                    return "/images/placeholder.png";
                
                // Map department to folder path
                var folderPath = Department switch
                {
                    "CIVIL ENGINEERING TOOLROOM" => "Civil Engineering/CE tools",
                    "ELECTRONICS ENGINEERING TOOLROOM" => "Electronics/Electronics Tool",
                    "CHEMISTRY LABORATORY TOOLROOM" => "Chemistry",
                    "ELECTRICAL ENGINEERING TOOLROOM" => "Electrical/Electrical Tools",
                    "PHYSICS LABORATORY TOOLROOM" => "Physics/Physics Tool",
                    _ => ""
                };
                
                // For Chemistry, use uppercase item name with spaces (matching file names)
                // Format: /images/Apparatus/{folderPath}/{prefix}-{ItemName}.png
                string itemNameForPath = Department == "CHEMISTRY LABORATORY TOOLROOM" 
                    ? Name.ToUpper() 
                    : Name;
                
                var imagePath = $"/images/Apparatus/{folderPath}/{departmentPrefix}-{itemNameForPath}.png";
                return imagePath;
            }
        }
    }
}

