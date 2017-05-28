using System.Linq;
using Hunspell.NetCore.Infrastructure;

namespace Hunspell.NetCore
{
    public sealed class CharacterConditionGroup : ArrayWrapper<CharacterCondition>
    {
        public static readonly CharacterConditionGroup Empty = TakeArray(ArrayEx<CharacterCondition>.Empty);

        public static readonly CharacterConditionGroup AllowAnySingleCharacter = Create(CharacterCondition.AllowAny);

        public static readonly ArrayWrapperComparer<CharacterCondition, CharacterConditionGroup> DefaultComparer = new ArrayWrapperComparer<CharacterCondition, CharacterConditionGroup>();

        private CharacterConditionGroup(CharacterCondition[] conditions)
            : base(conditions)
        {
        }

        public bool AllowsAnySingleCharacter => Items.Length == 1 && Items[0].AllowsAny;

        internal static CharacterConditionGroup TakeArray(CharacterCondition[] conditions) => conditions == null ? Empty : new CharacterConditionGroup(conditions);

        public static CharacterConditionGroup Create(CharacterCondition condition) => TakeArray(new[] { condition });

        public string GetEncoded() => string.Concat(Items.Select(c => c.GetEncoded()));

        public override string ToString() => GetEncoded();

        /// <summary>
        /// Determines if the start of the given <paramref name="text"/> matches the conditions.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True when the start of the <paramref name="text"/> is matched by the conditions.</returns>
        public bool IsStartingMatch(string text)
        {
            if (string.IsNullOrEmpty(text) || Items.Length > text.Length)
            {
                return false;
            }

            for (int i = 0; i < Items.Length; i++)
            {
                if (!Items[i].IsMatch(text[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if the end of the given <paramref name="text"/> matches the conditions.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True when the end of the <paramref name="text"/> is matched by the conditions.</returns>
        public bool IsEndingMatch(string text)
        {
            if (Items.Length > text.Length)
            {
                return false;
            }

            for (int conditionIndex = Items.Length - 1, textIndex = text.Length - 1; conditionIndex >= 0; conditionIndex--, textIndex--)
            {
                if (!Items[conditionIndex].IsMatch(text[textIndex]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if the end of the given <paramref name="text"/> matches the conditions.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True when the end of the <paramref name="text"/> is matched by the conditions.</returns>
        internal bool IsEndingMatch(StringSlice text)
        {
            if (Items.Length > text.Length)
            {
                return false;
            }

            for (int conditionIndex = Items.Length - 1, textIndex = text.Length - 1; conditionIndex >= 0; conditionIndex--, textIndex--)
            {
                if (!Items[conditionIndex].IsMatch(text.Text[text.Offset + textIndex]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsOnlyPossibleMatch(string text)
        {
            if (string.IsNullOrEmpty(text) || Items.Length != text.Length)
            {
                return false;
            }

            for (var i = 0; i < text.Length; i++)
            {
                var condition = Items[i];
                if (!condition.PermitsSingleCharacter || condition.Characters[0] != text[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
