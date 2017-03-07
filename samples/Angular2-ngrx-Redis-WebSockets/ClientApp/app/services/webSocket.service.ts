import { Injectable } from '@angular/core'
import { QueueingSubject } from 'queueing-subject'
import { Observable } from 'rxjs/Observable'
import websocketConnect from 'rxjs-websockets'
import 'rxjs/Rx';

@Injectable()
export class WebSocketService {
    private inputStream: QueueingSubject<any>
    private socket: any;

    public messages: Observable<any>
    public connectionStatus: Observable<any>
    public isConnected: boolean = false;

    constructor() {
        this.connect();
        //    .subscribe((status: boolean) => {
        //    this.isConnected = status;
        //    console.log('Connection status: ', status)
        //})
    }

    public connect(): Observable<any> {
        if (this.messages)
            return

        // Using share() causes a single websocket to be created when the first
        // observer subscribes. This socket is shared with subsequent observers
        // and closed when the observer count falls to zero.
        //this.socket = websocketConnect(
        //    'ws://localhost:5000/dataSocket',
        //    this.inputStream = new QueueingSubject<any>()
        //);


        this.messages = websocketConnect(
            'ws://localhost:5000/dataSocket',
            this.inputStream = new QueueingSubject<any>()
        ).messages.share();

        //this.messages = this.socket.messages.share()
        //this.connectionStatus = this.socket.connectionStatus.share()
        //return this.connectionStatus;
    }

    public send(message: any): void {
        // If the websocket is not connected then the QueueingSubject will ensure
        // that messages are queued and delivered when the websocket reconnects.
        // A regular Subject can be used to discard messages sent when the websocket
        // is disconnected.
        console.log('message sent: ', message)

        this.inputStream.next(message)
    }
}