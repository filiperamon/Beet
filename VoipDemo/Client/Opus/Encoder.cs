using System;

using Android.Runtime;

namespace Legion.Client.Opus
{
    public class Encoder
    {
        private long State;

        public int Bitrate
        {
            get { return OpusLibrary.Java_aopus_OpusLibrary_encoderGetBitrate(JNIEnv.Handle, IntPtr.Zero, State); }
            set { OpusLibrary.Java_aopus_OpusLibrary_encoderSetBitrate(JNIEnv.Handle, IntPtr.Zero, State, value); }
        }

        public double Quality
        {
            get { return OpusLibrary.Java_aopus_OpusLibrary_encoderGetQuality(JNIEnv.Handle, IntPtr.Zero, State); }
            set { OpusLibrary.Java_aopus_OpusLibrary_encoderSetQuality(JNIEnv.Handle, IntPtr.Zero, State, Quality); }
        }

        public Encoder(int clockRate, int channels, int packetTime)
        {
            try
            {
                State = OpusLibrary.Java_aopus_OpusLibrary_encoderCreate(JNIEnv.Handle, IntPtr.Zero, clockRate, channels, packetTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ActivateFEC(int packetLossPercent)
        {
            OpusLibrary.Java_aopus_OpusLibrary_encoderActivateFEC(JNIEnv.Handle, IntPtr.Zero, State, packetLossPercent);
        }

        public void DeactivateFEC()
        {
            OpusLibrary.Java_aopus_OpusLibrary_encoderDeactivateFEC(JNIEnv.Handle, IntPtr.Zero, State);
        }

        public void Destroy()
        {
            try
            {
                OpusLibrary.Java_aopus_OpusLibrary_encoderDestroy(JNIEnv.Handle, IntPtr.Zero, State);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public byte[] Encode(byte[] data, int index, int length)
        {
            IntPtr dataPtr = JNIEnv.NewArray(data);
            try
            {
                return (byte[])JNIEnv.GetArray(OpusLibrary.Java_aopus_OpusLibrary_encoderEncode(JNIEnv.Handle, IntPtr.Zero, State, dataPtr, index, length), JniHandleOwnership.TransferLocalRef, typeof(byte));
            }
            finally
            {
                JNIEnv.DeleteLocalRef(dataPtr);
            }
        }
    }
}