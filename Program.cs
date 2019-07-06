// Decompiled with JetBrains decompiler
// Type: HCHO_TEST.Program
// Assembly: HCHO_TEST, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC436C84-C200-4B69-921B-D64C128EA012
// Assembly location: C:\Users\heave\Desktop\hcho上位机\HCHO_TEST.exe

using System;
using System.Windows.Forms;

namespace HCHO_TEST
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
