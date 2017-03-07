import { Component } from '@angular/core';
import { Store } from "@ngrx/store";
import { connectToServer } from '../../app.actions'

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})

export class AppComponent {
    constructor(public store: Store<{}>) {
        store.dispatch(connectToServer());
    }
}
