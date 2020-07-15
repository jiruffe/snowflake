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
        private const long EPOCH = 1577836800000L;

        #region Bits allocations for timestamp, dataCenterID, machineID and sequence.

        private const long UNUSED_BITS = 1;

        /// <summary>
        /// 'timestamp' here is defined as the number of millisecond that have
        /// elapsed since the <see cref="EPOCH"/> given by users on <see cref="Snowflake"/>
        /// instance initialization.
        /// </summary>
        private const long TIMESTAMP_BITS = 41;

        private const long DATA_CENTER_ID_BITS = 5;
        private const long MACHINE_ID_BITS = 5;
        private const long SEQUENCE_BITS = 12;

        #endregion

        #region Max values of dataCenterID, machineID and sequence.

        private const long MAX_DATA_CENTER_NUM = ~(-1L << DATA_CENTER_ID_BITS);     // 2^5-1
        private const long MAX_MACHINE_NUM = ~(-1L << MACHINE_ID_BITS);             // 2^5-1
        private const long MAX_SEQUENCE = ~(-1L << SEQUENCE_BITS);                  // 2^12-1

        #endregion

        #region Left shift bits of timestamp, dataCenterID and machineID.

        private const long MACHINE_ID_SHIFT = SEQUENCE_BITS;
        private const long DATA_CENTER_ID_SHIFT = SEQUENCE_BITS + MACHINE_ID_BITS;
        private const long TIMESTAMP_SHIFT = DATA_CENTER_ID_SHIFT + DATA_CENTER_ID_BITS;

        #endregion

        #endregion

        #region Fields

        /// <summary>
        /// Data center number the process running on, its value can't be modified after initialization.
        /// <para>
        /// max: 2^5-1 range: [0,31]
        /// </para>
        /// </summary>
        private readonly long DataCenterID { public get; };

        /// <summary>
        /// Machine or process number, its value can't be modified after initialization.
        /// <para>
        /// max: 2^5-1 range: [0,31]
        /// </para>
        /// </summary>
        private readonly long MachineID { public get; };

        /// <summary>
        /// The unique and incrementing sequence number scoped in only one
        /// period/unit (here is ONE millisecond). its value will be increased by 1
        /// in the same specified period and then reset to 0 for next period.
        /// <para>
        /// max: 2^12-1 range: [0,4095]
        /// </para>
        /// </summary>
        private long Sequence = 0L { private get; private set; };

        /// <summary>
        /// The timestamp last snowflake ID generated.
        /// </summary>
        private long LastTimestamp = -1L { private get; private set; };

        #endregion

        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="DataCenterID">data center number the process running on, value range: [0,31]</param>
        /// <param name="MachineID">machine or process number, value range: [0,31]</param>
        public Snowflake(long DataCenterID, long MachineID) {
            if (DataCenterID > MAX_DATA_CENTER_NUM || DataCenterID < 0) {
                throw new IllegalArgumentException(
                        String.Format("DataCenterID can't be greater than MAX_DATA_CENTER_NUM ( %d ) or less than 0", MAX_DATA_CENTER_NUM));
            }
            if (MachineID > MAX_MACHINE_NUM || MachineID < 0) {
                throw new IllegalArgumentException(
                        String.Format("MachineID can't be greater than MAX_MACHINE_NUM ( %d ) or less than 0", MAX_MACHINE_NUM));
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
        public long NextID() {
            long CurrentTimestamp = GetTimestamp();
            if (CurrentTimestamp < LastTimestamp) {
                throw new RuntimeException("Clock moved backwards. Refusing to generate ID");
            }

            if (CurrentTimestamp == LastTimestamp) {
                // same period/millisecond, increasing
                Sequence = (Sequence + 1) & MAX_SEQUENCE;
                // overflow: greater than max sequence
                if (Sequence == 0L) {
                    CurrentTimestamp = GetNextMillisecond(CurrentTimestamp);
                }
            } else {
                // reset to 0 for next period/millisecond
                Sequence = 0L;
            }

            LastTimestamp = CurrentTimestamp;

            return (CurrentTimestamp - EPOCH) << TIMESTAMP_SHIFT
                    | dataCenterID << DATA_CENTER_ID_SHIFT
                    | machineID << MACHINE_ID_SHIFT
                    | Sequence;
        }

        #endregion

    }

}