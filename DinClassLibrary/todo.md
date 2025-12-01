# (**Note, this todo list was AI generated with limited context, it most likely guessed and halucinated some things.**)

### Project Status Overview

Currently, you have the **basic skeleton of the resolution engine** and a **hardcoded HTTP server**. You are missing the "Framework" elements (automation, generic handling, lifecycle management, and performance optimization).

---

### Todo List Evaluation

#### 1. Assembly Scanning
- [ ] **Not Done**
    - **Analysis:** Your `DinContainer` currently requires manual registration via `Register(Type serviceType, Type implementationType)`. There is no code that automatically scans an assembly for types.
    - **How to do it:** Create a method like `RegisterControllers(Assembly assembly)`. Iterate through `assembly.GetTypes()`, check if they follow a convention (e.g., inherit from a `Controller` base class or end with "Controller"), and call `Register` for each.

#### 2. Singleton Support
- [ ] **Not Done**
    - **Analysis:** Your `GetService` method creates a **new instance** (`constructor.Invoke(args)`) every time it is called. This is "Transient" behavior.
    - **How to do it:** You need to modify `_registry` or create a new dictionary (e.g., `_singletons`) to store instantiated objects. When `GetService` is called, check if the instance already exists. If yes, return it; if no, create it, save it, and then return it.

#### 3. Constructor Injection
- [x] **Done** (mostly)
    - **Location:** `DinContainer.cs` inside `GetService`.
    - **Analysis:**
        - You correctly identify constructors: `registryValue.GetConstructors()`.
        - You filter for parameters: `c.GetParameters().Length != 0`.
        - You throw an error if there are multiple candidates (Ambiguity check).
        - You support parameters and recursive dependency resolution.
    - **Note:** Ensure your error messages are descriptive as requested by the assignment.

#### 4. Controller Support
- [ ] **Not Done** (Too specific/Hardcoded)
    - **Location:** `DinHttpListener.cs`.
    - **Analysis:** Your `DinHttpListener` is not a generic framework; it is a hardcoded application. It specifically checks for `segments[1].Equals("numbers")` and looks for specific methods like `GetAll`. A framework should not know about "numbers".
    - **How to do it:**
        - The `DinHttpListener` should take the `DinContainer` as a dependency.
        - When a request comes in (e.g., `/api/users`), it should use reflection to find a class named `UsersController` in the container.
        - It should dynamically find the method matching the HTTP verb or attribute, not via a `switch` statement on specific strings.

#### 5. Dynamic Interception (AOP)
- [ ] **Not Done**
    - **Analysis:** There is no code related to `DispatchProxy`, `RealProxy`, or dynamic class generation.
    - **How to do it:** Look into `System.Reflection.DispatchProxy`. You need to wrap the service instance in a proxy that intercepts method calls to print the Logs/Timing before and after the actual method execution.

#### 6. Dependency Graph & Cycle Detection
- [~] **Partially Done**
    - **Location:** `DinDependencyGraph.cs` and `DinContainer.cs`.
    - **Analysis:** You are successfully **building** the graph using QuikGraph (`_dependencyGraph.AddEdge` inside `GetService`).
    - **Missing:** You are **not detecting** cycles. If Service A depends on B, and B depends on A, your `GetService` will crash with a `StackOverflowException` before the graph is fully updated or analyzed.
    - **How to do it:** You need to check for cycles *before* or *during* resolution. Since you are using QuikGraph, you can use built-in algorithms (like `DirectedAcyclicGraph` checks) or keep a `Stack<Type>` of the current resolution chain to detect if you see the same type twice in the same branch.

#### 7. Logging Framework
- [ ] **Not Done**
    - **Analysis:** You are using `Console.WriteLine` directly.
    - **How to do it:** Define a simple `ILogger` interface or use a configurable implementation. You need to support "levels" so the user can turn off the verbose "Resolving dependency..." logs if they want to.

#### 8. Performance (Minimize Reflection)
- [ ] **Not Done**
    - **Analysis:** In `GetService`, you call `GetConstructors()` and `GetParameters()` every single time an object is requested. This is slow.
    - **How to do it:** Cache the constructor metadata. When a type is registered, look up its constructor *once* and store a `Func<object>` or `ConstructorInfo` in a dictionary so you don't have to reflect on the type every time `GetService` is called.

#### 9. Demo Application
- [ ] **Not Done**
    - **Analysis:** Code provided is only the library/listener.

#### 10. Architecture (SRP)
- [~] **Mixed**
    - **Analysis:** `DinContainer` is decent. However, `DinHttpListener` violates SRP heavily. It handles HTTP listening, Routing, JSON serialization, and Business Logic (the "numbers" logic) all in one class.

#### 11. Unit Tests
- [ ] **Not Done**

#### 12. Solution & README
- [ ] **Not Done**

---

### Immediate Next Steps for You

1.  **Refactor the HTTP Listener:** Make it generic. It should not contain the word "numbers". It should parse the URL, look up a Controller from the `DinContainer`, and invoke a method.
2.  **Implement Singleton Lifetime:** Change `Register` to accept a lifetime (Transient vs Singleton) and update `GetService` to store instances.
3.  **Implement Cycle Detection:** Since you are already building the graph, write a method `ValidateGraph()` that runs a cycle detection algorithm from QuikGraph.

**Would you like me to show you how to implement the `DispatchProxy` for the Interception (AOP) requirement?**