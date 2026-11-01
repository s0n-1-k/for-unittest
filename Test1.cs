using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathExpressionsLab;

namespace MathExpressionsLab.Tests
{
    [TestClass]
    public class AddTests
    {
        // ---------- 1. Вычисление ----------

        [TestMethod]
        public void Compute_TwoConstants_ReturnsSum()
        {
            // Arrange
            var expr = new Add(new Constant(2), new Constant(3));

            // Act
            double actual = expr.Compute(new Dictionary<string, double>());

            // Assert
            Assert.AreEqual(5.0, actual, 1e-9);
        }

        [TestMethod]
        public void Compute_WithVariables_UsesProvidedValues()
        {
            // Arrange
            var x = new Variable("x");
            var y = new Variable("y");
            var expr = new Add(x, y); // x + y

            var values = new Dictionary<string, double> { ["x"] = 1.5, ["y"] = 2.5 };

            // Act
            double actual = expr.Compute(values);

            // Assert
            Assert.AreEqual(4.0, actual, 1e-9);
        }

        [TestMethod]
        public void Compute_NestedAdds_ReturnsCorrectResult()
        {
            // Arrange: (1 + 2) + (3 + 4)
            var expr = new Add(
                new Add(new Constant(1), new Constant(2)),
                new Add(new Constant(3), new Constant(4))
            );

            // Act
            double actual = expr.Compute(new Dictionary<string, double>());

            // Assert
            Assert.AreEqual(10.0, actual, 1e-9);
        }

        // ---------- 2. Метаданные ----------

        [TestMethod]
        public void Variables_ReturnsUnionOfLeftAndRight()
        {
            // Arrange
            var expr = new Add(new Variable("x"), new Variable("y"));

            // Act
            var vars = new List<string>(expr.Variables);

            // Assert
            CollectionAssert.AreEquivalent(new[] { "x", "y" }, vars);
        }

        [TestMethod]
        public void Variables_DuplicatesRemoved()
        {
            // Arrange: x + x
            var expr = new Add(new Variable("x"), new Variable("x"));

            // Act
            var vars = new List<string>(expr.Variables);

            // Assert
            Assert.AreEqual(1, vars.Count);
            Assert.AreEqual("x", vars[0]);
        }

        [TestMethod]
        public void IsConstant_TrueOnlyWhenBothOperandsConstant()
        {
            Assert.IsTrue(new Add(new Constant(1), new Constant(2)).IsConstant);
            Assert.IsFalse(new Add(new Constant(1), new Variable("x")).IsConstant);
            Assert.IsFalse(new Add(new Variable("x"), new Variable("y")).IsConstant);
        }

        [TestMethod]
        public void IsPolynomial_TrueWhenBothOperandsPolynomial()
        {
            Assert.IsTrue(new Add(new Constant(1), new Variable("x")).IsPolynomial);
            Assert.IsTrue(new Add(new Variable("x"), new Variable("y")).IsPolynomial);

            // Sqrt(x) не полином (аргумент — переменная)
            Assert.IsFalse(new Add(new Sqrt(new Variable("x")), new Constant(1)).IsPolynomial);
        }

        [TestMethod]
        public void PolynomialDegree_IsMaxOfOperands()
        {
            // x + x^2 (степени 1 и 2) -> 2
            var x = new Variable("x");
            var expr = new Add(x, new Multiply(x, x));

            Assert.AreEqual(2, expr.PolynomialDegree);
        }

        [TestMethod]
        public void PolynomialDegree_WithConstantOnly_IsZero()
        {
            Assert.AreEqual(0, new Add(new Constant(1), new Constant(2)).PolynomialDegree);
        }

        // ---------- 3. Форматирование ----------

        [TestMethod]
        public void ToString_SimpleConstants_FormatsWithSpaces()
        {
            var expr = new Add(new Constant(1), new Constant(2));

            Assert.AreEqual("1 + 2", expr.ToString());
        }

        [TestMethod]
        public void ToString_NestedAdd_NoParensAroundSamePrecedence()
        {
            // (a + b) + c  ->  "a + b + c"  (левая ассоциативность, скобки не нужны)
            var a = new Variable("a");
            var b = new Variable("b");
            var c = new Variable("c");
            var expr = new Add(new Add(a, b), c);

            Assert.AreEqual("a + b + c", expr.ToString());
        }

        [TestMethod]
        public void ToString_AddInsideSubtractRight_Parenthesized()
        {
            // a - (b + c)  ->  правая часть должна быть в скобках
            var a = new Variable("a");
            var b = new Variable("b");
            var c = new Variable("c");
            var expr = new Subtract(a, new Add(b, c));

            Assert.AreEqual("a - (b + c)", expr.ToString());
        }

        [TestMethod]
        public void ToString_NegativeConstant_Parenthesized()
        {
            // (-3) + x  ->  Constant(-3) выводится как "(-3)"
            var expr = new Add(new Constant(-3), new Variable("x"));

            Assert.AreEqual("(-3) + x", expr.ToString());
        }

        // ---------- 4. Перегрузка оператора и неявное преобразование ----------

        [TestMethod]
        public void OperatorPlus_CreatesAddExpression()
        {
            var x = new Variable("x");

            Expr expr = x + 5; // неявное преобразование 5 -> Constant(5)

            Assert.IsInstanceOfType(expr, typeof(Add));
            Assert.AreEqual("x + 5", expr.ToString());
            Assert.AreEqual(7.0, expr.Compute(new Dictionary<string, double> { ["x"] = 2 }), 1e-9);
        }

        // ---------- 5. Обработка ошибок ----------

        [TestMethod]
        public void Compute_MissingVariableValue_Throws()
        {
            // Arrange
            var expr = new Add(new Variable("x"), new Variable("y"));
            var values = new Dictionary<string, double> { ["x"] = 1 };

            // Act & Assert (проверяем, что вызов бросает нужное исключение)
            Assert.ThrowsExactly<KeyNotFoundException>(() => expr.Compute(values));
        }
        [TestMethod]
        public void Compute_ExistingVariable_ReturnsValue()
        {
            // Arrange
            var x = new Variable("x");
            var values = new Dictionary<string, double> { ["x"] = 42.5 };

            // Act
            double actual = x.Compute(values);

            // Assert
            Assert.AreEqual(42.5, actual, 1e-9);
        }
    }
}