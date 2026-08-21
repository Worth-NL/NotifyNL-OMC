import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Both pages are client-only (EventSource/fetch against the OMC API), so a
  // static export lets the .NET Dockerfile bake this app straight into the
  // same image/container OMC already ships as — no separate Node server.
  output: "export",
  trailingSlash: true,
};

export default nextConfig;
