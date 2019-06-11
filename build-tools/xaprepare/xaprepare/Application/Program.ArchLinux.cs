using System;
using System.Threading.Tasks;

namespace Xamarin.Android.Prepare
{
	class ArchLinuxProgram : LinuxProgram
	{
		public ArchLinuxProgram (string packageName, string executableName = null)
			: base (packageName, executableName)
		{}

		protected override bool CheckWhetherInstalled ()
		{
			var runner = new ProcessRunner ("pacman", "-Q", "--", PackageName);

			if (!runner.Run()) {
				Log.Error ($"Check for package {PackageName} failed");
				return false;
			}

			return runner.ExitCode == 0;
		}

		public override async Task<bool> Install ()
		{
			var runner = new ProcessRunner ("sudo", "pacman", "-S", "--", PackageName) {
				EchoStandardOutput = true,
				EchoStandardError = true,
				ProcessTimeout = TimeSpan.FromMinutes (30),
			};

			bool failed = await Task.Run (() => !runner.Run ());
			if (failed) {
				Log.Error ($"Installation of {PackageName} timed out");
				failed = true;
			}

			if (runner.ExitCode != 0) {
				Log.Error ($"Installation failed with error code {runner.ExitCode}");
				failed = true;
			}

			return !failed;
		}

		protected override bool DeterminePackageVersion()
		{
			string currentVersion = Utilities.GetStringFromStdout ("pacman", "-Q", "--", PackageName);

			int index = currentVersion.IndexOf (' ');
			if (index < 0) {
				Log.Error ($"Could not determine version of {PackageName}");
				return false;
			}

			CurrentVersion = currentVersion.Substring (PackageName.Length + 1).Trim ();
			return true;
		}
	}
}
