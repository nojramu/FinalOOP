using System.ComponentModel.DataAnnotations;

namespace SmartQuiz.Models.Base
{
    /// <summary>
    /// Abstract base class for all entities in the system.
    /// Demonstrates: ABSTRACTION - defines common structure for all entities
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; } // Public setter for Entity Framework compatibility

        /// <summary>
        /// Abstract method that must be implemented by derived classes.
        /// Demonstrates: ABSTRACTION - forces derived classes to define validation
        /// </summary>
        public abstract bool Validate();

        /// <summary>
        /// Virtual method that can be overridden by derived classes.
        /// Demonstrates: POLYMORPHISM - allows different implementations
        /// </summary>
        public virtual string GetDisplayName()
        {
            return $"Entity #{Id}";
        }
    }
}

