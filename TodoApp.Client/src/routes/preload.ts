export const preloadAuthenticatedRoutes = () => {
  void Promise.all([
    import("../components/PrivateRoute"),
    import("../layouts/MainLayout"),
    import("../pages/Dashboard"),
  ]);
};

export const scheduleAuthenticatedRoutesPreload = () => {
  const requestIdleCallback = window.requestIdleCallback;

  if (typeof requestIdleCallback === "function") {
    requestIdleCallback(preloadAuthenticatedRoutes, { timeout: 1500 });
    return;
  }

  globalThis.setTimeout(preloadAuthenticatedRoutes, 300);
};
