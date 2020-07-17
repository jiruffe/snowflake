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

#include "snowflake.h"

#ifndef SNOWFLAKE_CONSTANTS
#define SNOWFLAKE_CONSTANTS

/*
 * Reference material of 'timestamp' is '2020-01-01 00:00:00 GMT'.
 * Its value can't be modified after initialization.
 */
#ifndef SNOWFLAKE_EPOCH
#define SNOWFLAKE_EPOCH 1577836800000UL
#endif

/*
 * Bits allocations for timestamp, data_center_id, machine_id and sequence.
 */
#ifndef SNOWFLAKE_UNUSED_BITS
#define SNOWFLAKE_UNUSED_BITS 1
#endif
/*
 * 'timestamp' here is defined as the number of millisecond that have
 * elapsed since the {@link #EPOCH} given by users on {@link Snowflake}
 * instance initialization.
 */
#ifndef SNOWFLAKE_TIMESTAMP_BITS
#define SNOWFLAKE_TIMESTAMP_BITS 41
#endif
#ifndef SNOWFLAKE_DATA_CENTER_ID_BITS
#define SNOWFLAKE_DATA_CENTER_ID_BITS 5
#endif
#ifndef SNOWFLAKE_MACHINE_ID_BITS
#define SNOWFLAKE_MACHINE_ID_BITS 5
#endif
#ifndef SNOWFLAKE_SEQUENCE_BITS
#define SNOWFLAKE_SEQUENCE_BITS 12
#endif

/*
 * Max values of data_center_id, machine_id and sequence.
 */
#ifndef SNOWFLAKE_MAX_DATA_CENTER_ID
#define SNOWFLAKE_MAX_DATA_CENTER_ID (~(SNOWFLAKE_ID_MAX << SNOWFLAKE_DATA_CENTER_ID_BITS))
#endif
#ifndef SNOWFLAKE_MAX_MACHINE_ID
#define SNOWFLAKE_MAX_MACHINE_ID (~(SNOWFLAKE_ID_MAX << SNOWFLAKE_MACHINE_ID_BITS))
#endif
#ifndef SNOWFLAKE_MAX_SEQUENCE
#define SNOWFLAKE_MAX_SEQUENCE (~(SNOWFLAKE_ID_MAX << SNOWFLAKE_SEQUENCE_BITS))
#endif

/*
 * Left shift bits of timestamp, data_center_id and machine_id.
 */
#ifndef SNOWFLAKE_
#define SNOWFLAKE_
#endif

#ifndef SNOWFLAKE_
#define SNOWFLAKE_
#endif

#ifndef SNOWFLAKE_
#define SNOWFLAKE_
#endif

#endif // SNOWFLAKE_CONSTANTS
