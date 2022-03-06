﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

namespace FluffySpoon.AspNet.Ngrok
{
	public class NgrokOptions
	{
		public bool Disable { get; set; }

		public bool ShowNgrokWindow { get; set; }

		/// <summary>
		/// Sets the local URL Ngrok will proxy to. Must be http (not https) at this time. If not filled in, it will be populated automatically.
		/// </summary>
		public string? ApplicationHttpUrl { get; set; }
	}
}
