#region License
/*
 *    MIT License
 *
 *    Copyright (c) 2020 Jiruffe
 *
 *    Permission is hereby granted, free of charge, to any person obtaining a copy
 *    of this software and associated documentation files (the "Software"), to deal
 *    in the Software without restriction, including without limitation the rights
 *    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *    copies of the Software, and to permit persons to whom the Software is
 *    furnished to do so, subject to the following conditions:
 *
 *    The above copyright notice and this permission notice shall be included in all
 *    copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *    SOFTWARE.
 */
#endregion

using System;
using System.Runtime.CompilerServices;

namespace Com.Jiruffe {

    /// <summary>
    /// C# edition of Twitter <c>Snowflake</c>, a network service for generating
    /// unique ID numbers at high scale with some simple guarantees.
    /// <para>
    /// https://github.com/twitter/snowflake
    /// </para>
    /// </summary>
    public sealed class Snowflake {

        #region Constants

        /// <summary>
        /// Reference material of 'timestamp' is '2020-01-01 00:00:00 GMT'.
        /// Its value can't be modified after initialization.
        /// </summary>
        private const ulong EPOCH = 1577836800000UL;

        #region Bits allocations for timestamp, dataCenterID, machineID and sequence.

        private const byte UNUSED_BITS = 1;

        /// <summary>
        /// 'timestamp' here is defined as the number of millisecond that have
        /// elapsed since the <see cref="EPOCH"/> given by users on <see cref="Snowflake"/>
        /// instance initialization.
        /// </summary>
        private const byte TIMESTAMP_BITS = 41;

        private const byte DATA_CENTER_ID_BITS = 5;
        private const byte MACHINE_ID_BITS = 5;
        private const byte SEQUENCE_BITS = 12;

        #endregion

        #region Max values of dataCenterID, machineID and sequence.

        private const ulong MAX_DATA_CENTER_ID = ~(ulong.MaxValue << DATA_CENTER_ID_BITS);     // 2^5-1
        private const ulong MAX_MACHINE_NUM = ~(ulong.MaxValue << MACHINE_ID_BITS);             // 2^5-1
        private const ulong MAX_SEQUENCE = ~(ulong.MaxValue << SEQUENCE_BITS);                  // 2^12-1

        #endregion

        #region Left shift bits of timestamp, dataCenterID and machineID.

        private const byte MACHINE_ID_SHIFT = SEQUENCE_BITS;
        private const byte DATA_CENTER_ID_SHIFT = SEQUENCE_BITS + MACHINE_ID_BITS;
        private const byte TIMESTAMP_SHIFT = DATA_CENTER_ID_SHIFT + DATA_CENTER_ID_BITS;

        #endregion

        #endregion

        #region Fields

        /// <summary>
        /// Data center number the process running on, its value can't be modified after initialization.
        /// <para>
        /// Max: 2^5-1 Range: [0,31]
        /// </para>
        /// </summary>
        private readonly ulong DataCenterID;

        /// <summary>
        /// Machine or process number, its value can't be modified after initialization.
        /// <para>
        /// Max: 2^5-1 Range: [0,31]
        /// </para>
        /// </summary>
        private readonly ulong MachineID;

        /// <summary>
        /// The unique and incrementing sequence number scoped in only one
        /// period/unit (here is ONE millisecond). its value will be increased by 1
        /// in the same specified period and then reset to 0 for next period.
        /// <para>
        /// max: 2^12-1 range: [0,4095]
        /// </para>
        /// </summary>
        private ulong Sequence = 0UL;

        /// <summary>
        /// The timestamp last snowflake ID generated.
        /// </summary>
        private ulong LastTimestamp = 0UL;

        #endregion

        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="DataCenterID">
        /// Data center number the process running on, value range: [0,31]
        /// </param>
        /// <param name="MachineID">
        /// Machine or process number, value range: [0,31]
        /// </param>
        public Snowflake(ulong DataCenterID, ulong MachineID) {
            if (DataCenterID > MAX_DATA_CENTER_ID || DataCenterID < 0UL) {
                throw new ArgumentOutOfRangeException(
                        string.Format("DataCenterID can't be greater than MAX_DATA_CENTER_ID ( {0} ) or less than 0", MAX_DATA_CENTER_ID));
            }
            if (MachineID > MAX_MACHINE_NUM || MachineID < 0UL) {
                throw new ArgumentOutOfRangeException(
                        string.Format("MachineID can't be greater than MAX_MACHINE_NUM ( {0} ) or less than 0", MAX_MACHINE_NUM));
            }
            this.DataCenterID = DataCenterID;
            this.MachineID = MachineID;
        }

        #endregion

        #region Indexers

        #endregion

        #region Accessors

        #endregion

        #region Methods

        /// <summary>
        /// Generates an unique and incrementing ID.
        /// </summary>
        /// <returns>
        /// ID
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ulong NextID() {
            ulong CurrentTimestamp = GetTimestamp();
            if (CurrentTimestamp < LastTimestamp) {
                throw new Exception("Clock moved backwards. Refusing to generate ID");
            }

            if (CurrentTimestamp == LastTimestamp) {
                // same period/millisecond, increasing
                Sequence = (Sequence + 1) & MAX_SEQUENCE;
                // overflow: greater than max sequence
                if (Sequence == 0UL) {
                    CurrentTimestamp = GetNextMillisecond(CurrentTimestamp);
                }
            } else {
                // reset to 0 for next period/millisecond
                Sequence = 0UL;
            }

            LastTimestamp = CurrentTimestamp;

            return (CurrentTimestamp - EPOCH) << TIMESTAMP_SHIFT
                    | DataCenterID << DATA_CENTER_ID_SHIFT
                    | MachineID << MACHINE_ID_SHIFT
                    | Sequence;
        }

        private DateTime GetDateTimeWhenUnixTimestampStarts() {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Running loop blocking until next millisecond.
        /// </summary>
        /// <param name="CurrentTimestamp">
        /// Current timestamp
        /// </param>
        /// <returns>
        /// Current timestamp in millisecond
        /// </returns>
        private ulong GetNextMillisecond(ulong CurrentTimestamp) {
            while (CurrentTimestamp <= LastTimestamp) {
                CurrentTimestamp = GetTimestamp();
            }
            return CurrentTimestamp;
        }

        /// <summary>
        /// Gets current timestamp.
        /// </summary>
        /// <returns>
        /// Current timestamp in millisecond
        /// </returns>
        private ulong GetTimestamp() {
            return Convert.ToUInt64((DateTime.UtcNow - GetDateTimeWhenUnixTimestampStarts()).TotalMilliseconds);
        }

        /// <summary>
        /// A diode is a long value whose left and right margin are ZERO, while
        /// middle bits are ONE in binary string layout. It looks like a diode in
        /// shape.
        /// </summary>
        /// <param name="Offset">
        /// Left margin position
        /// </param>
        /// <param name="Length">
        /// Offset + Length is right margin position
        /// </param>
        /// <returns>
        /// A long value
        /// </returns>
        private static ulong Diode(byte Offset, byte Length) {
            int lb = 64 - Offset;
            int rb = 64 - (Offset + Length);
            return (ulong.MaxValue << lb) ^ (ulong.MaxValue << rb);
        }

        /// <summary>
        /// Extract Timestamp, DataCenterID, MachineID and Sequence number
        /// information from the given ID.
        /// </summary>
        /// <param name="ID">
        /// A Snowflake ID generated by this object
        /// </param>
        /// <returns>
        /// An array containing Timestamp, DataCenterID, MachineID and Sequence number
        /// </returns>
        public ulong[] ParseID(ulong ID) {
            ulong[] arr = new ulong[5];
            arr[4] = ((ID & Diode(UNUSED_BITS, TIMESTAMP_BITS)) >> TIMESTAMP_SHIFT);
            arr[0] = arr[4] + EPOCH;
            arr[1] = (ID & Diode(UNUSED_BITS + TIMESTAMP_BITS, DATA_CENTER_ID_BITS)) >> DATA_CENTER_ID_SHIFT;
            arr[2] = (ID & Diode(UNUSED_BITS + TIMESTAMP_BITS + DATA_CENTER_ID_BITS, MACHINE_ID_BITS)) >> MACHINE_ID_SHIFT;
            arr[3] = (ID & Diode(UNUSED_BITS + TIMESTAMP_BITS + DATA_CENTER_ID_BITS + MACHINE_ID_BITS, SEQUENCE_BITS));
            return arr;
        }

        /// <summary>
        /// Extract and display Timestamp, DataCenterID, MachineID and Sequence
        /// number information from the given ID in humanization format
        /// </summary>
        /// <param name="ID">
        /// A Snowflake ID in ulong format
        /// </param>
        /// <returns>
        /// A Snowflake id in String format
        /// </returns>
        public string FormatID(ulong ID) {
            ulong[] arr = ParseID(ID);
            string tmf = GetDateTimeWhenUnixTimestampStarts().AddMilliseconds(arr[0]).ToString("yyyy-MM-dd HH:mm:ss.fff");
            return string.Format("{0}, #{1}, @({2}, {3})", tmf, arr[3], arr[1], arr[2]);
        }

        #region Overrides object

        /// <summary>
        /// Show settings of this snowflake instance.
        /// </summary>
        /// <returns>
        /// A string represents settings of this snowflake instance.
        /// </returns>
        public override string ToString() {
            return "{\"Snowflake\":{" +
                "\"EPOCH\":" + EPOCH +
                ",\"UNUSED_BITS\":" + UNUSED_BITS +
                ",\"TIMESTAMP_BITS\":" + TIMESTAMP_BITS +
                ",\"DATA_CENTER_ID_BITS\":" + DATA_CENTER_ID_BITS +
                ",\"MACHINE_ID_BITS\":" + MACHINE_ID_BITS +
                ",\"SEQUENCE_BITS\":" + SEQUENCE_BITS +
                ",\"MAX_DATA_CENTER_ID\":" + MAX_DATA_CENTER_ID +
                ",\"MAX_MACHINE_NUM\":" + MAX_MACHINE_NUM +
                ",\"MAX_SEQUENCE\":" + MAX_SEQUENCE +
                ",\"MACHINE_ID_SHIFT\":" + MACHINE_ID_SHIFT +
                ",\"DATA_CENTER_ID_SHIFT\":" + DATA_CENTER_ID_SHIFT +
                ",\"TIMESTAMP_SHIFT\":" + TIMESTAMP_SHIFT +
                ",\"DataCenterID\":" + DataCenterID +
                ",\"MachineID\":" + MachineID +
                ",\"Sequence\":" + Sequence +
                ",\"LastTimestamp\":" + LastTimestamp +
                "}}";
        }

        #endregion

        #endregion

    }

}
