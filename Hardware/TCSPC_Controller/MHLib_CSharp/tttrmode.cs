/************************************************************************

  C# demo access to MultiHarp 150 Hardware via MHLIB v 1.0.
  The program performs a measurement based on hardcoded settings.

  The resulting event data is stored in a binary output file.

  PicoQuant GmbH, Sept. 2018

  Note: This is a console application (i.e. run in Windows cmd box)

  Note: At the API level channel numbers are indexed 0..N-1 
		where N is the number of channels the device has.

  
  Tested with the following compilers:

  - Mono 5.14.0.177
  - MS Visual Studio 2013 and 2015

************************************************************************/

using System;  // for Console
using System.Text;  // for StringBuilder 
using System.IO;  // for File
using System.Runtime.InteropServices;  // for DllImport

namespace tttrmode
{
  class tttrmode
  {
    static void Main(string[] args)
    {
      int i, j;
      int retcode;
      int[] dev = new int[Constants.MAXDEVNUM];
      int found = 0;
      int NumChannels = 0;

      StringBuilder LibVer = new StringBuilder(8);
      StringBuilder Serial = new StringBuilder(8);
      StringBuilder Errstr = new StringBuilder(40);
      StringBuilder Model = new StringBuilder(16);
      StringBuilder Partno = new StringBuilder(8);
      StringBuilder Version = new StringBuilder(16);
      StringBuilder Wtext = new StringBuilder(16384);

      int Mode = Constants.MODE_T2;  // you can change this, adjust other settings accordingly!
      int Binning = 0;  // you can change this, meaningful only in T3 mode, observe limits 
      int Offset = 0;  // you can change this, meaningful only in T3 mode, observe limits 
      int Tacq = 10000;  // Measurement time in millisec, you can change this, observe limits 

      int SyncDivider = 1;  // you can change this, usually 1 in T2 mode 

      int SyncTrigEdge = 0;  // you can change this, observe limits
      int SyncTrigLevel = -50;  // you can change this, observe limits
      int InputTrigEdge = 0;  // you can change this, observe limits
      int InputTrigLevel = -50;  // you can change this, observe limits

      double Resolution = 0;

      int Syncrate = 0;
      int Countrate = 0;
      int ctcstatus = 0;
      int flags = 0;
      long Progress = 0;
      int nRecords = 0;
      int warnings = 0;

      uint[] buffer = new uint[Constants.TTREADMAX];

      FileStream fs = null;
      BinaryWriter bw = null;


      Console.WriteLine("MultiHarp 150     MHLib Demo Application               PicoQuant GmbH, 2018");
      Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

      retcode = mhlib.MH_GetLibraryVersion(LibVer);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_GetLibraryVersion error {0}. Aborted.", Errstr);
        goto ex;
      }
      Console.WriteLine("MHLib Version is " + LibVer);

      if (LibVer.ToString() != Constants.LIB_VERSION)
      {
        Console.WriteLine("This program requires MHLib v." + Constants.LIB_VERSION);
        goto ex;
      }

      try
      {
        fs = File.Create("tttrmode.out");
        bw = new BinaryWriter(fs);
      }
      catch (Exception)
      {
        Console.WriteLine("Error creating file");
        goto ex;
      }

      Console.WriteLine("Searching for MultiHarp devices...");
      Console.WriteLine("Devidx     Status");


      for (i = 0; i < Constants.MAXDEVNUM; i++)
      {
        retcode = mhlib.MH_OpenDevice(i, Serial);
        if (retcode == 0) //Grab any device we can open
        {
          Console.WriteLine("  {0}        S/N {1}", i, Serial);
          dev[found] = i; //keep index to devices we want to use
          found++;
        }
        else
        {

          if (retcode == Errorcodes.MH_ERROR_DEVICE_OPEN_FAIL)
            Console.WriteLine("  {0}        no device", i);
          else
          {
            mhlib.MH_GetErrorString(Errstr, retcode);
            Console.WriteLine("  {0}        S/N {1}", i, Errstr);
          }
        }
      }

      //In this demo we will use the first device we find, i.e. dev[0].
      //You can also use multiple devices in parallel.
      //You can also check for specific serial numbers, so that you always know 
      //which physical device you are talking to.

      if (found < 1)
      {
        Console.WriteLine("No device available.");
        goto ex;
      }

      Console.WriteLine("Using device {0}", dev[0]);
      Console.WriteLine("Initializing the device...");

      retcode = mhlib.MH_Initialize(dev[0], Constants.MODE_T2, 0);  // Histo mode
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_Initialize error {0}. Aborted.", Errstr);
        goto ex;
      }

      retcode = mhlib.MH_GetHardwareInfo(dev[0], Model, Partno, Version);  // this is only for information
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_GetHardwareInfo error {0}. Aborted.", Errstr);
        goto ex;
      }
      else
        Console.WriteLine("Found Model {0} Part no {1} Version {2}", Model, Partno, Version);


      retcode = mhlib.MH_GetNumOfInputChannels(dev[0], ref NumChannels);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_GetNumOfInputChannels error {0}. Aborted.", Errstr);
        goto ex;
      }
      else
        Console.WriteLine("Device has {0} input channels.", NumChannels);

      Console.WriteLine("\n\nUsing the following settings:\n");

      Console.WriteLine("Mode              : " + Mode);
      Console.WriteLine("Binning           : " + Binning);
      Console.WriteLine("Offset            : " + Offset);
      Console.WriteLine("AcquisitionTime   : " + Tacq);
      Console.WriteLine("SyncDivider       : " + SyncDivider);

      retcode = mhlib.MH_SetSyncDiv(dev[0], SyncDivider);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_SetSyncDiv Error {0}. Aborted.", Errstr);
        goto ex;
      }

      retcode = mhlib.MH_SetSyncEdgeTrg(dev[0], SyncTrigLevel, SyncTrigEdge);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_SetSyncEdgeTrg Error {0}. Aborted.", Errstr);
        goto ex;
      }

      retcode = mhlib.MH_SetSyncChannelOffset(dev[0], 0);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_SetSyncChannelOffset Error {0}. Aborted.", Errstr);
        goto ex;
      }

      for (i = 0; i < NumChannels; i++)  // we use the same input settings for all channels
      {
        retcode = mhlib.MH_SetInputEdgeTrg(dev[0], i, InputTrigLevel, InputTrigEdge);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine("MH_SetInputEdgeTrg Error {0}. Aborted.", Errstr);
          goto ex;
        }

        retcode = mhlib.MH_SetInputChannelOffset(dev[0], i, 0);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine("MH_SetInputChannelOffset Error {0}. Aborted.", Errstr);
          goto ex;
        }
        retcode = mhlib.MH_SetInputChannelEnable(dev[0], i, 1);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine("MH_SetInputChannelEnable Error {0}. Aborted.", Errstr);
          goto ex;
        }
      }

      if (Mode != Constants.MODE_T2)
      {
        retcode = mhlib.MH_SetBinning(dev[0], Binning);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine("MH_SetBinning Error {0}. Aborted.", Errstr);
          goto ex;
        }

        retcode = mhlib.MH_SetOffset(dev[0], Offset);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine("MH_SetOffset Error {0}. Aborted.", Errstr);
          goto ex;
        }
      }

      retcode = mhlib.MH_GetResolution(dev[0], ref Resolution);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_GetResolution Error {0}. Aborted.", Errstr);
        goto ex;
      }

      Console.WriteLine("Resolution is {0} ps", Resolution);

      // After Init allow 150 ms for valid  count rate readings
      // Subsequently you get new values after every 100ms
      System.Threading.Thread.Sleep(150);

      retcode = mhlib.MH_GetSyncRate(dev[0], ref Syncrate);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_GetSyncRate Error {0}. Aborted.", Errstr);
        goto ex;
      }
      Console.WriteLine("Syncrate = {0}/s", Syncrate);

      for (i = 0; i < NumChannels; i++)  // for all channels
      {
        retcode = mhlib.MH_GetCountRate(dev[0], i, ref Countrate);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine("MH_GetCountRate Error {0}. Aborted.", Errstr);
          goto ex;
        }
        Console.WriteLine("Countrate[{0}] = {1}/s", i, Countrate);
      }

      Console.WriteLine();

      // After getting the count rates you can check for warnings
      retcode = mhlib.MH_GetWarnings(dev[0], ref warnings);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_GetWarnings Error {0}. Aborted.", Errstr);
        goto ex;
      }
      if (warnings != 0)
      {
        mhlib.MH_GetWarningsText(dev[0], Wtext, warnings);
        Console.WriteLine("{0}", Wtext);
      }


      Progress = 0;
      Console.Write("Progress: {0,12}", Progress);


      retcode = mhlib.MH_StartMeas(dev[0], Tacq);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine();
        Console.WriteLine("MH_StartMeas Error {0}. Aborted.", Errstr);
        goto ex;
      }

      while (true)
      {
        retcode = mhlib.MH_GetFlags(dev[0], ref flags);
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine();
          Console.WriteLine("MH_GetFlags Error {0}. Aborted.", Errstr);
          goto ex;
        }

        if ((flags & Constants.FLAG_FIFOFULL) != 0)
        {
          Console.WriteLine();
          Console.WriteLine("FiFo Overrun!");
          goto stoptttr;
        }

        retcode = mhlib.MH_ReadFiFo(dev[0], buffer, ref nRecords);  // may return less!
        if (retcode < 0)
        {
          mhlib.MH_GetErrorString(Errstr, retcode);
          Console.WriteLine();
          Console.WriteLine("MH_GetFlags Error {0}. Aborted.", Errstr);
          goto ex;
        }

        if (nRecords > 0)
        {

          for (j = 0; j < nRecords; j++)
            bw.Write(buffer[j]);

          Progress += nRecords;
          Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b{0,12}", Progress);
        }
        else
        {
          retcode = mhlib.MH_CTCStatus(dev[0], ref ctcstatus);
          if (retcode < 0)
          {
            mhlib.MH_GetErrorString(Errstr, retcode);
            Console.WriteLine();
            Console.WriteLine("MH_CTCStatus Error {0}. Aborted.", Errstr);
            goto ex;
          }
          if (ctcstatus > 0)
          {
            Console.WriteLine();
            Console.WriteLine("Done");
            goto stoptttr;
          }
        }

        // within this loop you can also read the count rates if needed.
      }

      stoptttr:
      Console.WriteLine();

      retcode = mhlib.MH_StopMeas(dev[0]);
      if (retcode < 0)
      {
        mhlib.MH_GetErrorString(Errstr, retcode);
        Console.WriteLine("MH_StopMeas Error {0}. Aborted.", Errstr);
        goto ex;
      }

      bw.Close();
      fs.Close();

      ex:

      for (i = 0; i < Constants.MAXDEVNUM; i++)  // no harm to close all
      {
        mhlib.MH_CloseDevice(i);
      }

      Console.WriteLine("press RETURN to exit");
      Console.ReadLine();
    }
  }
}

