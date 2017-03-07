import { Component, OnInit, OnDestroy } from '@angular/core';
import { Http } from '@angular/http';

import { Subscription } from 'rxjs/Subscription'
import { WebSocketService } from '../../services/webSocket.service'
import { DataOperationRequest } from '../../dataAccess/dataOperationRequest';

@Component({
    selector: 'fetchdata',
    templateUrl: './fetchdata.component.html'
})
export class FetchDataComponent implements OnInit, OnDestroy {
    private socketSubscription: Subscription;
    private connectionStatus: boolean = false;

    public forecasts: WeatherForecast[];

    constructor(private socket: WebSocketService) { }

    ngOnInit() {
        /*this.socket.connect()*/;

        this.socketSubscription = this.socket.messages.subscribe((message: any) => {
            console.log('received message from server: ', message)
        })

        // send message to server, if the socket is not connected it will be sent
        // as soon as the connection becomes available thanks to QueueingSubject
        let dor = new DataOperationRequest();
        dor.operationId = (Math.random() * 100000).toString();
        dor.controller = "WebSocketsSample.Controllers.SampleDataController";
        dor.action = "WeatherForecasts";
        //dor.arguments.push({ message: "text 1" });

        this.socket.send(dor);
    }

    ngOnDestroy() {
        //this.socketSubscription.unsubscribe()
    }
}

export interface WeatherForecast {
    dateFormatted: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}
