using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TestExamIS.Tests.Utils
{
    public static class ArchitectureChecker
    {
        private static readonly Assembly[] _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        private static readonly Type _dbContextBaseType = typeof(IdentityDbContext);

        public static void CheckControllerForOnionArchitectureCompliance(string controllerName)
        {
            try
            {
                var violations = CheckControllerArchitecture(controllerName);

                if (violations.Any())
                    throw new InvalidOperationException(
                        $"Cannot run functional tests for {controllerName} because it violates architecture rules. " +
                        $"Fix the architecture violations first:\n{string.Join("\n", violations)}");
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                var errorMessage = $"Failed to check architecture for {controllerName}: {ex.Message}";
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
        
        public static void CheckServiceForOnionArchitectureCompliance(string serviceName)
        {
            try
            {
                var violations = CheckServiceDbContextDependency(serviceName);

                if (!violations.Any())
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"Cannot run functional tests for {serviceName} because it violates architecture rules. " +
                    $"Fix the architecture violations first:\n{string.Join("\n", violations)}");
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                var errorMessage = $"Failed to check architecture for {serviceName}: {ex.Message}";
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// Checks if a specific controller violates the architecture rule by depending directly on DbContext
        /// </summary>
        /// <param name="controllerName">Name of the controller to check (e.g., "FlightsController")</param>
        /// <returns>List of violations, empty if no violations found</returns>
        public static List<string> CheckControllerDbContextDependency(string controllerName)
        {
            var violations = new List<string>();

            var controllerType = _allAssemblies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .FirstOrDefault(t => t.Name == controllerName);

            if (controllerType == null)
            {
                violations.Add($"Controller '{controllerName}' not found in loaded assemblies");
                return violations;
            }

            var dbContextType = FindDbContextType();
            if (dbContextType == null)
            {
                violations.Add("Could not find ApplicationDbContext or any DbContext subclass");
                return violations;
            }

            var constructors = controllerType.GetConstructors();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType == dbContextType ||
                        _dbContextBaseType.IsAssignableFrom(parameter.ParameterType))
                    {
                        violations.Add(
                            $"Controller '{controllerName}' has a direct dependency on '{parameter.ParameterType.Name}'");
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Checks if a specific controller violates the architecture rule by depending directly on repository interfaces
        /// </summary>
        /// <param name="controllerName">Name of the controller to check</param>
        /// <returns>List of violations, empty if no violations found</returns>
        public static List<string> CheckControllerRepositoryDependency(string controllerName)
        {
            var violations = new List<string>();

            var controllerType = _allAssemblies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .FirstOrDefault(t => t.Name == controllerName);

            if (controllerType == null)
            {
                violations.Add($"Controller '{controllerName}' not found in loaded assemblies");
                return violations;
            }

            var repositoryInterfaceTypes = GetRepositoryInterfaces().ToList();

            var constructors = controllerType.GetConstructors();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                foreach (var parameter in parameters)
                {
                    if (repositoryInterfaceTypes.Contains(parameter.ParameterType) ||
                        repositoryInterfaceTypes.Any(ri => ri.IsAssignableFrom(parameter.ParameterType)))
                    {
                        violations.Add(
                            $"Controller '{controllerName}' has a direct dependency on repository interface '{parameter.ParameterType.Name}'");
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Checks if a specific service violates the architecture rule by depending directly on DbContext
        /// </summary>
        /// <param name="serviceName">Name of the service to check</param>
        /// <returns>List of violations, empty if no violations found</returns>
        public static List<string> CheckServiceDbContextDependency(string serviceName)
        {
            var violations = new List<string>();

            var serviceType = _allAssemblies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .FirstOrDefault(t => t.Name == serviceName);

            if (serviceType == null)
            {
                violations.Add($"Service '{serviceName}' not found in loaded assemblies");
                return violations;
            }

            var dbContextType = FindDbContextType();
            if (dbContextType == null)
            {
                violations.Add("Could not find ApplicationDbContext or any DbContext subclass");
                return violations;
            }

            var constructors = serviceType.GetConstructors();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType == dbContextType ||
                        _dbContextBaseType.IsAssignableFrom(parameter.ParameterType))
                    {
                        violations.Add(
                            $"Service '{serviceName}' has a direct dependency on '{parameter.ParameterType.Name}'");
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Performs all architecture checks for a specific controller. Checks if the Controller depends on DbContext or IRepository
        /// </summary>
        /// <param name="controllerName">Name of the controller to check</param>
        /// <returns>Combined list of all violations</returns>
        public static List<string> CheckControllerArchitecture(string controllerName)
        {
            var allViolations = new List<string>();

            allViolations.AddRange(CheckControllerDbContextDependency(controllerName));
            allViolations.AddRange(CheckControllerRepositoryDependency(controllerName));

            return allViolations;
        }

        #region Helper Methods

        private static Type FindDbContextType()
        {
            var dbContextType = _allAssemblies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .FirstOrDefault(t => t.Name.Contains("ApplicationDbContext"));

            if (dbContextType == null)
            {
                dbContextType = _allAssemblies
                    .SelectMany(a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch
                        {
                            return Type.EmptyTypes;
                        }
                    })
                    .FirstOrDefault(t => _dbContextBaseType.IsAssignableFrom(t) && t != _dbContextBaseType);
            }

            return dbContextType;
        }

        private static IEnumerable<Type> GetRepositoryInterfaces()
        {
            return _allAssemblies
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Repository"));
        }

        #endregion
    }
}