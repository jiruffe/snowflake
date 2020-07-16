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

package com.jiruffe;

import java.text.SimpleDateFormat;
import java.util.Date;

/**
 * Java edition of Twitter <b>Snowflake</b>, a network service for generating
 * unique ID numbers at high scale with some simple guarantees.
 * <p>
 * https://github.com/twitter/snowflake
 */
public class Snowflake {

    /**
     * Reference material of 'timestamp' is '2020-01-01 00:00:00 GMT'.
     * Its value can't be modified after initialization.
     */
    private final long EPOCH = 1577836800000L;

    /*
     * Bits allocations for timestamp, dataCenterID, machineID and sequence.
     */
    private final long UNUSED_BITS = 1;
    /**
     * 'timestamp' here is defined as the number of millisecond that have
     * elapsed since the {@link #EPOCH} given by users on {@link Snowflake}
     * instance initialization.
     */
    private final long TIMESTAMP_BITS = 41;
    private final long DATA_CENTER_ID_BITS = 5;
    private final long MACHINE_ID_BITS = 5;
    private final long SEQUENCE_BITS = 12;

    /*
     * Max values of dataCenterID, machineID and sequence.
     */
    private final long MAX_DATA_CENTER_NUM = ~(-1L << DATA_CENTER_ID_BITS);     // 2^5-1
    private final long MAX_MACHINE_NUM = ~(-1L << MACHINE_ID_BITS);             // 2^5-1
    private final long MAX_SEQUENCE = ~(-1L << SEQUENCE_BITS);                  // 2^12-1

    /*
     * Left shift bits of timestamp, dataCenterID and machineID.
     */
    private final long MACHINE_ID_SHIFT = SEQUENCE_BITS;
    private final long DATA_CENTER_ID_SHIFT = SEQUENCE_BITS + MACHINE_ID_BITS;
    private final long TIMESTAMP_SHIFT = DATA_CENTER_ID_SHIFT + DATA_CENTER_ID_BITS;

    /**
     * Data center number the process running on, its value can't be modified after initialization.
     * <p>
     * max: 2^5-1 range: [0,31]
     */
    private final long dataCenterID;

    /**
     * Machine or process number, its value can't be modified after initialization.
     * <p>
     * max: 2^5-1 range: [0,31]
     */
    private final long machineID;

    /**
     * The unique and incrementing sequence number scoped in only one
     * period/unit (here is ONE millisecond). its value will be increased by 1
     * in the same specified period and then reset to 0 for next period.
     * <p>
     * max: 2^12-1 range: [0,4095]
     */
    private long sequence = 0L;

    /**
     * The timestamp last snowflake ID generated.
     */
    private long lastTimestamp = -1L;

    /**
     * @param dataCenterID data center number the process running on, value range: [0,31]
     * @param machineID    machine or process number, value range: [0,31]
     */
    public Snowflake(long dataCenterID, long machineID) {
        if (dataCenterID > MAX_DATA_CENTER_NUM || dataCenterID < 0) {
            throw new IllegalArgumentException(
                    String.format("dataCenterID can't be greater than MAX_DATA_CENTER_NUM ( %d ) or less than 0", MAX_DATA_CENTER_NUM));
        }
        if (machineID > MAX_MACHINE_NUM || machineID < 0) {
            throw new IllegalArgumentException(
                    String.format("machineID can't be greater than MAX_MACHINE_NUM ( %d ) or less than 0", MAX_MACHINE_NUM));
        }
        this.dataCenterID = dataCenterID;
        this.machineID = machineID;
    }

    /**
     * Generates an unique and incrementing ID.
     *
     * @return ID
     */
    public synchronized long nextID() {
        long currentTimestamp = getTimestamp();
        if (currentTimestamp < lastTimestamp) {
            throw new RuntimeException("Clock moved backwards. Refusing to generate ID");
        }

        if (currentTimestamp == lastTimestamp) {
            // same period/millisecond, increasing
            sequence = (sequence + 1) & MAX_SEQUENCE;
            // overflow: greater than max sequence
            if (sequence == 0L) {
                currentTimestamp = getNextMillisecond(currentTimestamp);
            }
        } else {
            // reset to 0 for next period/millisecond
            sequence = 0L;
        }

        lastTimestamp = currentTimestamp;

        return (currentTimestamp - EPOCH) << TIMESTAMP_SHIFT
                | dataCenterID << DATA_CENTER_ID_SHIFT
                | machineID << MACHINE_ID_SHIFT
                | sequence;
    }

    /**
     * Running loop blocking until next millisecond.
     *
     * @param currentTimestamp current timestamp
     * @return current timestamp in millisecond
     */
    private long getNextMillisecond(long currentTimestamp) {
        while (currentTimestamp <= lastTimestamp) {
            currentTimestamp = getTimestamp();
        }
        return currentTimestamp;
    }

    /**
     * Gets current timestamp.
     *
     * @return current timestamp in millisecond
     */
    private long getTimestamp() {
        return System.currentTimeMillis();
    }

    /**
     * A diode is a long value whose left and right margin are ZERO, while
     * middle bits are ONE in binary string layout. It looks like a diode in
     * shape.
     *
     * @param offset left margin position
     * @param length offset+length is right margin position
     * @return a long value
     */
    private static long diode(long offset, long length) {
        int lb = (int) (64 - offset);
        int rb = (int) (64 - (offset + length));
        return (-1L << lb) ^ (-1L << rb);
    }

    /**
     * Extract timestamp, dataCenterID, machineID and sequence number
     * information from the given ID.
     *
     * @param id a snowflake ID generated by this object
     * @return an array containing timestamp, dataCenterID, machineID and sequence number
     */
    public long[] parseID(long id) {
        long[] arr = new long[5];
        arr[4] = ((id & diode(UNUSED_BITS, TIMESTAMP_BITS)) >> TIMESTAMP_SHIFT);
        arr[0] = arr[4] + EPOCH;
        arr[1] = (id & diode(UNUSED_BITS + TIMESTAMP_BITS, DATA_CENTER_ID_BITS)) >> DATA_CENTER_ID_SHIFT;
        arr[2] = (id & diode(UNUSED_BITS + TIMESTAMP_BITS + DATA_CENTER_ID_BITS, MACHINE_ID_BITS)) >> MACHINE_ID_SHIFT;
        arr[3] = (id & diode(UNUSED_BITS + TIMESTAMP_BITS + DATA_CENTER_ID_BITS + MACHINE_ID_BITS, SEQUENCE_BITS));
        return arr;
    }

    /**
     * Extract and display timestamp, dataCenterID, machineID and sequence
     * number information from the given id in humanization format
     *
     * @param id snowflake id in Long format
     * @return snowflake id in String format
     */
    public String formatID(long id) {
        long[] arr = parseID(id);
        String tmf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS").format(new Date(arr[0]));
        return String.format("%s, #%d, @(%d, %d)", tmf, arr[3], arr[1], arr[2]);
    }

    /**
     * Show settings of this snowflake instance.
     * @return a string represents settings of this snowflake instance.
     */
    @Override
    public String toString() {
        return "{\"Snowflake\":{" +
                "\"EPOCH\":" + EPOCH +
                ",\"UNUSED_BITS\":" + UNUSED_BITS +
                ",\"TIMESTAMP_BITS\":" + TIMESTAMP_BITS +
                ",\"DATA_CENTER_ID_BITS\":" + DATA_CENTER_ID_BITS +
                ",\"MACHINE_ID_BITS\":" + MACHINE_ID_BITS +
                ",\"SEQUENCE_BITS\":" + SEQUENCE_BITS +
                ",\"MAX_DATA_CENTER_NUM\":" + MAX_DATA_CENTER_NUM +
                ",\"MAX_MACHINE_NUM\":" + MAX_MACHINE_NUM +
                ",\"MAX_SEQUENCE\":" + MAX_SEQUENCE +
                ",\"MACHINE_ID_SHIFT\":" + MACHINE_ID_SHIFT +
                ",\"DATA_CENTER_ID_SHIFT\":" + DATA_CENTER_ID_SHIFT +
                ",\"TIMESTAMP_SHIFT\":" + TIMESTAMP_SHIFT +
                ",\"dataCenterID\":" + dataCenterID +
                ",\"machineID\":" + machineID +
                ",\"sequence\":" + sequence +
                ",\"lastTimestamp\":" + lastTimestamp +
                "}}";
    }

}
