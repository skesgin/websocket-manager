import { Action } from '@ngrx/store';
import { ActionTypes } from './app.actions';

export function reducer(state, action) {
    switch (action.type) {
        case ActionTypes.CONNECT_TO_SERVER: {

        }


        default:
            return state;
    }
}