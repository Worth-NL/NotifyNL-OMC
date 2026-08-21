import Link from "next/link";
import { OMC_API_URL } from "@/lib/api";

interface TopBarLink {
  href: string;
  label: string;
  external?: boolean;
}

export function TopBar({ links }: { links: TopBarLink[] }) {
  return (
    <nav className="flex h-14 items-center justify-between bg-orange px-8">
      <a
        className="flex items-center gap-2.5 text-[0.85rem] font-bold tracking-wider text-white uppercase"
        href="https://www.notificatie.nl"
        target="_blank"
        rel="noreferrer"
      >
        <svg
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2.2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        Notificatie.nl — OMC
      </a>
      <div className="flex gap-2">
        {links.map((link) =>
          link.external ? (
            <a
              key={link.href}
              className="rounded-md border border-white/40 px-3.5 py-1.5 text-[0.8rem] font-medium text-white/85 transition hover:bg-white/15 hover:text-white"
              href={link.href}
              target="_blank"
              rel="noreferrer"
            >
              {link.label}
            </a>
          ) : (
            <Link
              key={link.href}
              className="rounded-md border border-white/40 px-3.5 py-1.5 text-[0.8rem] font-medium text-white/85 transition hover:bg-white/15 hover:text-white"
              href={link.href}
            >
              {link.label}
            </Link>
          ),
        )}
      </div>
    </nav>
  );
}

export function swaggerLink() {
  return `${OMC_API_URL}/swagger`;
}
