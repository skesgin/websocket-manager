import { Action } from "@ngrx/store";
import { $WebSocket } from 'angular2-websocket/angular2-websocket'

export enum ActionTypes {
    PROCESS_DB_UPDATE,
    PROCESS_FILESYSTEM_CHANGE,
    PROCESS_NOTIFICATION,
    CONNECT_TO_SERVER
}

export function connectToServer(): Action {
    return {
        type: ActionTypes[ActionTypes.CONNECT_TO_SERVER],
        payload: {}
    }
}