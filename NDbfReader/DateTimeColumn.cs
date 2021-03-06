﻿using System;
using System.Diagnostics;

namespace NDbfReaderEx
{
    /// <summary>
    /// Represents a <see cref="Date"/> column.
    /// </summary>
    [DebuggerDisplay("DateTime {Name}")]
    public class DateTimeColumn : Column<DateTime>
    {
        /// <summary>
        /// Initializes a new instance with the specified name and offset.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <param name="offset">The column offset in a row in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is &lt; 0.</exception>
        public DateTimeColumn(string name, NativeColumnType dbfType, int offset)
            : base(name, dbfType, offset, 8, 0, null)
        {
        }

        /// <summary>
        /// Loads a value from the specified buffer.
        /// </summary>
        /// <param name="buffer">The byte array from which a value should be loaded. The buffer length is always at least equal to the column size.</param>
        /// <param name="encoding">The encoding that should be used when loading a value. The encoding is never <c>null</c>.</param>
        /// <returns>A column value.</returns>
        protected override DateTime ValueFromRowBuffer(byte[] rowBuffer, ref byte[] cachedColumnData)
        { // This didn't use cachedColumnData, it for MemoColumn only
            if (IsNull(rowBuffer))
            {
                return DateTime.MinValue;
            }

            var value1 = BitConverter.ToInt32(rowBuffer, offset_ + 1);
            var value2 = BitConverter.ToInt32(rowBuffer, offset_ + 5);

            // 2415021 = days between 1/1/4713 BC and 1/1/1900 AD
            var result = new DateTime(1900, 1, 1).AddDays(value1 - 2415021).AddMilliseconds(value2);

            if (result < new DateTime(1900, 1, 1))
            {
            }

            return result;
        }

        public override bool IsNull(byte[] rowBuffer)
        {
            for (int i = 0; i < size_; i++)
            {
                byte b = rowBuffer[offset_ + 1 + i];

                if (b == 0x00)
                { // not standard, but maybe a C/C++ EndOfString character used
                    break;
                }

                // disable for weird column
                //if (b == 0x3F)
                //{ // '?' character found
                //    break;
                //}

                if (b != 0x20)
                { // if contains any non blank character it isn't null value
                    return false;
                }
            }

            return true;
        }

        public override void SetNull(byte[] rowBuffer)
        {
            for (int i = 0; i < size_; i++)
            {
                rowBuffer[offset_ + 1 + i] = 0x20;
            }
        }
    }
}
