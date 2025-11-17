using SmartQuiz.Data;

namespace SmartQuiz.Services.Base
{
    /// <summary>
    /// Abstract base class for all services.
    /// Demonstrates: INHERITANCE - provides common functionality to all services
    /// Demonstrates: ABSTRACTION - defines common structure for services
    /// Demonstrates: COHESION - focused responsibility for service base functionality
    /// </summary>
    public abstract class BaseService
    {
        protected readonly DbContextFactory DbContextFactory;

        /// <summary>
        /// Encapsulation: Protected field accessible only to derived classes
        /// </summary>
        protected BaseService(DbContextFactory dbContextFactory)
        {
            DbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        /// <summary>
        /// Virtual method that can be overridden by derived classes.
        /// Demonstrates: POLYMORPHISM - allows different implementations
        /// </summary>
        protected virtual void LogOperation(string operation, string details)
        {
            Console.WriteLine($"[{GetType().Name}] {operation}: {details}");
        }

        /// <summary>
        /// Template method pattern - defines algorithm structure
        /// Demonstrates: ABSTRACTION - defines process without implementation details
        /// </summary>
        protected virtual T ExecuteWithErrorHandling<T>(Func<T> operation, T defaultValue)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                LogOperation("Error", ex.Message);
                return defaultValue;
            }
        }
    }
}

