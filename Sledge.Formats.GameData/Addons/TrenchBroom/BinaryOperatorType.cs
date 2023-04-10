namespace Sledge.Formats.GameData.Addons.TrenchBroom
{
    public enum BinaryOperatorType
    {
        // Math
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulus,
        
        // Logic
        LogicalAnd,
        LogicalOr,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseLeftShift,
        BitwiseRightShift,
        Implies,

        // Comparisons
        LessThan,
        LessThanEqual,
        GreaterThan,
        GreaterThanEqual,
        Equal,
        NotEqual,

        // Other
        Range,
    }
}