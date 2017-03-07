import 'rxjs/add/operator/mergeMap';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { Effect, Actions } from '@ngrx/effects';
import { ActionTypes } from './app.actions';
import { $WebSocket, WebSocketSendMode } from 'angular2-websocket/angular2-websocket';

@Injectable()
export class AppEffects {
    private ws: WebSocket;

    constructor(private actions$: Actions) {
        ;
    }

    @Effect()
    connectToServer$ = this.actions$
        .ofType(ActionTypes[ActionTypes.CONNECT_TO_SERVER])
        .mergeMap(() => Observable.create(this.ws = new WebSocket('ws://localhost:5000')));



    @Effect()
    processDbUpdate$ = this.actions$
        .ofType(ActionTypes[ActionTypes.PROCESS_DB_UPDATE])
        .mergeMap(() => Observable.create(this.ws = new WebSocket('ws://localhost:5000')));

}