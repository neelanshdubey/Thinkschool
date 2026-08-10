# Initial AI Generation Prompt

Create a deliberately bad legacy OrderController.cs for an ASP.NET Core 10 Web API application.

The purpose is to simulate a realistic two-year-old legacy controller that needs to be refactored.

Requirements:

1. Create approximately 300 lines of C# code.
2. Use a single giant POST /api/orders action.
3. Put business logic, validation, Entity Framework Core database access,
   calculations, and HTTP response shaping directly inside the controller action.
4. Use Entity Framework Core directly from the controller.
5. Make the action async, but intentionally use some synchronous EF Core
   operations such as ToList(), FirstOrDefault(), Find(), SaveChanges(),
   or similar synchronous database calls.
6. Include four separate empty catch { } blocks that swallow exceptions.
7. Return object instead of strongly typed IActionResult/ActionResult<T> responses.
8. Include poor separation of concerns.
9. Include duplicated logic.
10. Include hard-coded business rules or magic numbers.
11. Include weak/null validation.
12. Include at least one null dereference bug.
13. Include at least one subtle off-by-one bug.
14. Include poor error handling.
15. Include no cancellation token support.
16. Make the code look realistic rather than intentionally silly.
17. The controller should contain enough code to be approximately 300 lines.
18. Include simple Order, OrderItem, Product, and Customer models if needed.
19. Assume Entity Framework Core is being used.
20. Do not refactor the code.
21. Do not improve the architecture.
22. Do not add tests.
23. Save the controller as OrderController.cs.

The code should compile or be very close to compilable C# so that it can
serve as a realistic legacy starting point for a refactoring exercise.

Important:
Generate the legacy version only. Do not provide the refactored solution.