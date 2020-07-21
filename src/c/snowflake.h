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

/*
 * C edition of Twitter <b>Snowflake</b>, a network service for generating
 * unique ID numbers at high scale with some simple guarantees.
 *
 * https://github.com/twitter/snowflake
 */
#ifndef _SNOWFLAKE_H
#define _SNOWFLAKE_H

#include <stdlib.h>
#include <stddef.h>
#include <stdint.h>
#include <pthread.h>

#ifndef snowflake_id_t
#define snowflake_id_t uint_fast64_t
#endif

#ifndef SNOWFLAKE_ID_MAX
#define SNOWFLAKE_ID_MAX UINT_FAST64_MAX
#endif

typedef struct snowflake_t
{
    /*
     * Data center number the process running on, its value can't be modified after initialization.
     * max: 2^5-1 range: [0,31]
     */
    uint_fast64_t data_center_id;
    /*
     * Machine or process number, its value can't be modified after initialization.
     * max: 2^5-1 range: [0,31]
     */
    uint_fast64_t machine_id;
    /*
     * The unique and incrementing sequence number scoped in only one
     * period/unit (here is ONE millisecond). its value will be increased by 1
     * in the same specified period and then reset to 0 for next period.
     * max: 2^12-1 range: [0,4095]
     */
    uint_fast64_t sequence;
    /*
     * The timestamp last snowflake ID generated.
     */
    uint_fast64_t last_timestamp;
    /*
     * The mutex.
     */
    pthread_mutex_t * mutex;
} snowflake_t;

#endif // _SNOWFLAKE_H
