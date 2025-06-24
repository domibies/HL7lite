using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HL7lite
{
    internal class ElementCollection<T> : List<T> where T : MessageElement, new()
    {
        /// <summary>
        /// element indexer
        /// </summary>
        internal new T this[int index]
        {
            get
            {
                T element = null;

                if (index < base.Count)
                    element = base[index];

                return element;
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Add element at next position
        /// </summary>
        /// <param name="element">element</param>
        internal new void Add(T element)
        {
            base.Add(element);
        }

        /// <summary>
        /// Add element at specific position
        /// </summary>
        /// <param name="element">element</param>
        /// <param name="position">Position</param>
        internal void Add(T element, int position)
        {
            if (position < 1)
                throw new HL7Exception("Element position must be greater than or equal to 1");

            int listCount = base.Count;
            position = position - 1;

            if (position < listCount)
            {
                base[position] = element;
            }
            else
            {
                for (int comIndex = listCount; comIndex < position; comIndex++)
                {
                    T blankElement = new T();
                    blankElement.Encoding = element.Encoding;

                    blankElement.Value = string.Empty;
                    base.Add(blankElement);
                }

                base.Add(element);
            }
        }
    }
}
