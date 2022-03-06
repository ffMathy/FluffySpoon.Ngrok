// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NgrokExtensions

namespace FluffySpoon.AspNet.Ngrok.Models
{

	public class NgrokTunnelsApiResponse
	{
		public Tunnel[] Tunnels { get; set; }
		public string Uri { get; set; }
	}
}