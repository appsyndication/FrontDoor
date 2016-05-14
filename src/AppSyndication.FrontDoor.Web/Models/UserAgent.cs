using System;

namespace AppSyndication.Web.Models
{
    public enum Architecture
    {
        ARM,
        X86,
        X64,
        IA64,
    }

    public enum Platform
    {
        Unknown,
        Linux,
        MacOSX,
        Windows,
    }

    public class UserAgent
    {
        public UserAgent(string userAgent)
        {
            ParseArchitecture(userAgent);

            ParseOperatingSystem(userAgent);
        }

        public Architecture Architecture { get; private set; }

        public Platform Platform { get; private set; }

        public Version Version { get; private set; }

        private void ParseArchitecture(string userAgent)
        {
            if (-1 < userAgent.IndexOf("WOW64", StringComparison.OrdinalIgnoreCase) ||
                -1 < userAgent.IndexOf("Win64", StringComparison.OrdinalIgnoreCase) ||
                -1 < userAgent.IndexOf("x86_64", StringComparison.OrdinalIgnoreCase) ||
                -1 < userAgent.IndexOf("x64_64", StringComparison.OrdinalIgnoreCase) ||
                -1 < userAgent.IndexOf("AMD64", StringComparison.OrdinalIgnoreCase) ||
                -1 < userAgent.IndexOf("x64;", StringComparison.OrdinalIgnoreCase))
            {
                this.Architecture = Architecture.X64;
            }
            else if (-1 < userAgent.IndexOf("ia64", StringComparison.OrdinalIgnoreCase))
            {
                this.Architecture = Architecture.IA64;
            }
            else
            {
                this.Architecture = Architecture.X86;
            }
        }

        private void ParseOperatingSystem(string userAgent)
        {
            var str = userAgent.IndexOf("Windows NT", StringComparison.OrdinalIgnoreCase);

            if (str < 0)
            {
                str = userAgent.IndexOf("Mac OS X", StringComparison.OrdinalIgnoreCase);

                if (str < 0)
                {
                    str = userAgent.IndexOf("Linux", StringComparison.OrdinalIgnoreCase);

                    if (str < 0)
                    {
                        this.Platform = Platform.Unknown;

                        this.Version = new Version();
                    }
                    else // Linux
                    {
                        this.Platform = Platform.Linux;

                        this.Version = new Version();
                    }
                }
                else // Mac
                {
                    // TODO: Parse Mac OSX version.

                    this.Platform = Platform.MacOSX;

                    this.Version = new Version();
                }
            }
            else // Windows
            {
                var start = str + 11;
                var end = userAgent.IndexOfAny(new[] { ';', ')'}, str);
                var versionString = userAgent.Substring(start, end - start);

                this.Platform = Platform.Windows;

                this.Version = new Version(versionString);
            }
        }
    }
}