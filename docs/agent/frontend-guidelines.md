# Frontend Guidelines

## Purpose and Stack

The frontend is a polished, operational demo of the backend domain and workflows. Use Next.js, TypeScript, Tailwind CSS, shadcn/ui, TanStack Query, React Hook Form, and Zod.

Build product and admin screens, not a marketing-first landing page. Keep the UI quiet, professional, dense where operational work benefits, and consistent with `CONTEXT.md`.

## Implementation Rules

- Organize routes in `frontend/src/app` and feature code in `frontend/src/features`.
- Keep shadcn/ui primitives in `frontend/src/components/ui`, shared product components in `frontend/src/components`, and typed clients in `frontend/src/lib/api`.
- Use TanStack Query for server state and React Hook Form with Zod for forms.
- Provide loading, empty, error, and success states for server-backed screens.
- Make reservation states, inventory holds, Saga progress, event timelines, and Outbox status explicit and understandable.
- Keep tables and calendars scan-friendly; show meaningful labels instead of raw identifiers.

## Backend Boundary

Do not invent backend behavior for the UI. When an endpoint is missing, propose the contract before expanding backend scope. Mock only at a clearly marked boundary, keep mock data shaped like the intended response, and replace it with typed API calls when the endpoint exists.

If a frontend need introduces a new domain concept, pause and confirm that it belongs in the hotel domain before changing the backend.
