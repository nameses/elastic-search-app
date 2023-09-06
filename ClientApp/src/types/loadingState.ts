export type NotStarted = { type: 'notStarted' };
export type Loaded = { type: 'loaded' };
export type LoadedNotSuccessful = { type: 'loadedNotSuccessful' };
export type Loading = { type: 'loading' };

export type LoadingState = NotStarted | Loaded | LoadedNotSuccessful | Loading;
