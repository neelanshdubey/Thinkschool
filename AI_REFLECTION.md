# AI Reflection

## Claude Code

Claude Code was useful for identifying where the order business rules could be separated from the main service. The Strategy Pattern was appropriate because each validation rule can now be implemented independently through the IOrderRule interface. One important thing I would have caught during a diff review is unnecessary abstraction, such as introducing factories or additional layers when the service only needs IEnumerable<IOrderRule>.

The main risk in the refactoring is changing the order or behavior of validation rules. A strategy can be structurally correct while still changing existing application behavior, so I would verify the tests and compare the generated diff with the original implementation.

## Copilot

Copilot saved time by generating the repetitive unit-test structure from short comments. The negative-quantity, zero-quantity, and valid-quantity tests all follow the same Arrange-Act-Assert pattern, so generating the boilerplate was useful.

However, I would not accept a generated test without checking what it actually verifies. A subtly wrong test could pass because another validation rule throws the exception instead of the rule being tested. I would therefore make sure each test isolates the intended business rule and verifies the expected result.

## Production Use

At 2 AM IST while debugging production, I would reach for Claude first when I need repository-wide analysis or help understanding an unfamiliar code path. I would use Copilot more for focused tasks such as writing repetitive tests or completing small pieces of code.

The important lesson is that AI can accelerate implementation, but the developer still needs to understand the design, inspect the diff, and run the tests before accepting the change.