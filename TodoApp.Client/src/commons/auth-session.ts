const AUTHENTICATED_UNTIL_KEY = "todoapp:authenticated-until";
const AUTHENTICATION_GRACE_PERIOD_MS = 60_000;

export const markAuthenticated = () => {
  sessionStorage.setItem(
    AUTHENTICATED_UNTIL_KEY,
    String(Date.now() + AUTHENTICATION_GRACE_PERIOD_MS),
  );
};

export const clearAuthenticated = () => {
  sessionStorage.removeItem(AUTHENTICATED_UNTIL_KEY);
};

export const hasRecentAuthentication = () => {
  const authenticatedUntil = Number(sessionStorage.getItem(AUTHENTICATED_UNTIL_KEY));
  return Number.isFinite(authenticatedUntil) && authenticatedUntil > Date.now();
};
