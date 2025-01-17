using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Security;

namespace IRBTModUtils.Helper
{
    /// <summary>
    /// Provides direct formatting functions to speed up System.DateTime
    /// </summary>
    public static class FastFormatDate
    {
        // Outputs a string with DateTime.ToString("HH:mm:ss.fff ")

        /// <summary>
        /// Faster formatting function for "HH:mm:ss.fff ". Allocates new string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToHHmmssfff_(ref DateTime dateTime)
        {
            char* chars = stackalloc char[13];
            chars[0] = (char)((int)(dateTime.Hour / 10) + 48);
            chars[1] = (char)((dateTime.Hour % 10) + 48);
            chars[2] = ':';
            chars[3] = (char)((int)(dateTime.Minute / 10) + 48);
            chars[4] = (char)((dateTime.Minute % 10) + 48);
            chars[5] = ':';
            chars[6] = (char)((dateTime.Second / 10) + 48);
            chars[7] = (char)((dateTime.Second % 10) + 48);
            chars[8] = '.';
            chars[9] = (char)((dateTime.Millisecond / 100) + 48);
            chars[10] = (char)(((dateTime.Millisecond % 100) / 10) + 48);
            chars[11] = (char)(((dateTime.Millisecond % 10)) + 48);
            chars[12] = ' ';
            return new string(chars);
        }


        /// <summary>
        /// Faster formatting function for "[HH:mm:ss.fff]". Allocates new string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToHHmmssfffBR(ref DateTime dateTime)
        {
            char* chars = stackalloc char[14];
            chars[0] = '[';
            chars[1] = (char)((int)(dateTime.Hour / 10) + 48);
            chars[2] = (char)((dateTime.Hour % 10) + 48);
            chars[3] = ':';
            chars[4] = (char)((int)(dateTime.Minute / 10) + 48);
            chars[5] = (char)((dateTime.Minute % 10) + 48);
            chars[6] = ':';
            chars[7] = (char)((dateTime.Second / 10) + 48);
            chars[8] = (char)((dateTime.Second % 10) + 48);
            chars[9] = '.';
            chars[10] = (char)((dateTime.Millisecond / 100) + 48);
            chars[11] = (char)(((dateTime.Millisecond % 100) / 10) + 48);
            chars[12] = (char)(((dateTime.Millisecond % 10)) + 48);
            chars[13] = ']';
            return new string(chars);
        }

        /// <summary>
        /// Faster formatting function for "HH:mm:ss.ffff ". Allocates new string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressUnmanagedCodeSecurity]
        public unsafe static void ToHHmmssffff(ref DateTime dateTime, ref char[] chars)
        {

            chars[0] = (char)((int)(dateTime.Hour / 10) + 48);
            chars[1] = (char)((dateTime.Hour % 10) + 48);
            chars[2] = ':';
            chars[3] = (char)((int)(dateTime.Minute / 10) + 48);
            chars[4] = (char)((dateTime.Minute % 10) + 48);
            chars[5] = ':';
            chars[6] = (char)((dateTime.Second / 10) + 48);
            chars[7] = (char)((dateTime.Second % 10) + 48);
            chars[8] = '.';
            chars[9] = (char)((dateTime.Millisecond / 100) + 48);
            chars[10] = (char)(((dateTime.Millisecond % 100) / 10) + 48);
            chars[11] = (char)(((dateTime.Millisecond % 10)) + 48);
            chars[12] = (char)(((dateTime.Millisecond % 1)) + 48);
            chars[13] = ' ';
        }


        //"HH:mm:ss.fff"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void ToHHmmssfff(ref DateTime dateTime, ref char[] chars)
        {
            chars[0] = (char)((int)(dateTime.Hour / 10) + 48);
            chars[1] = (char)((dateTime.Hour % 10) + 48);
            chars[2] = ':';
            chars[3] = (char)((int)(dateTime.Minute / 10) + 48);
            chars[4] = (char)((dateTime.Minute % 10) + 48);
            chars[5] = ':';
            chars[6] = (char)((dateTime.Second / 10) + 48);
            chars[7] = (char)((dateTime.Second % 10) + 48);
            chars[8] = '.';
            chars[9] = (char)((dateTime.Millisecond / 100) + 48);
            chars[10] = (char)(((dateTime.Millisecond % 100) / 10) + 48);
            chars[11] = (char)(((dateTime.Millisecond % 10)) + 48);
            chars[12] = ' ';
        }

        //"HH-mm-ss"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void ToHHmmss(ref DateTime dateTime, ref char[] chars)
        {
            chars[0] = (char)((int)(dateTime.Hour / 10) + 48);
            chars[1] = (char)((dateTime.Hour % 10) + 48);
            chars[2] = '.';
            chars[3] = (char)((int)(dateTime.Minute / 10) + 48);
            chars[4] = (char)((dateTime.Minute % 10) + 48);
            chars[5] = '.';
            chars[6] = (char)((dateTime.Second / 10) + 48);
            chars[7] = (char)((dateTime.Second % 10) + 48);
            chars[8] = ' ';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToHHmmss(ref DateTime dateTime)
        {


            char* chars = stackalloc char[8];
            chars[0] = (char)((int)(dateTime.Hour / 10) + 48);
            chars[1] = (char)((dateTime.Hour % 10) + 48);
            chars[2] = '.';
            chars[3] = (char)((int)(dateTime.Minute / 10) + 48);
            chars[4] = (char)((dateTime.Minute % 10) + 48);
            chars[5] = '.';
            chars[6] = (char)((dateTime.Second / 10) + 48);
            chars[7] = (char)((dateTime.Second % 10) + 48);
            return new string(chars);
        }
    }
}