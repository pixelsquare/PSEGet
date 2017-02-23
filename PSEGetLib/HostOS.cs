using System;

namespace PSEGetLib
{
     public static class HostOS
     {
          /// <summary>
          /// an enumerator that holds a list of possible operating systems
          /// this code can execute on
          /// </summary>
          public enum HostEnviroment
          {
           Windows,
           Linux
          }
 
          /// <summary>
          /// determine the operating system this program is running on
          /// </summary>
          /// <returns>the current operating system</returns>
          /// <remarks>
          /// the current choices are only linux or windows
          /// it really only checks to see if the OS is linux,
          /// otherwise, it defaults to windows
          /// if other OS are inplemented in the future, this
          /// would need to be changed
          /// </remarks>
          public static HostEnviroment determineHostEnviroment()
          {
           HostEnviroment currentHost;
 
           int platformNumber = (int)Environment.OSVersion.Platform;
 
           //determine if the host is a *nix operating system
           //these numbers take into account historic values,
           //do an internet search for details
           if ((platformNumber == 4) || (platformNumber == 6)
           || (platformNumber == 128))
           {
            //running on *nix
            currentHost = HostEnviroment.Linux;
           }
           else
           {
            //not running on *nix (this function defaults to windows)
            currentHost = HostEnviroment.Windows;
           }
 
           return currentHost;
          }
 
      //............
     }
}

