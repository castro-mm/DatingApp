import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';
import { MessageParams } from '../_models/messageParams';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Group } from '../_models/group';

@Injectable({
    providedIn: 'root'
})
export class MessageService {
    baseApiUrl: string = environment.apiUrl;
    hubUrl: string = environment.hubUrl;
    private hubConnection?: HubConnection;
    private messageThreadSource = new BehaviorSubject<Message[]>([]);
    messageThread$ = this.messageThreadSource.asObservable();


    constructor(private http: HttpClient) { }

    createHubConnection(user: User, otherUsername: string) {
        this.hubConnection = new HubConnectionBuilder()
            .withUrl(this.hubUrl + 'message?user='+otherUsername, {
                accessTokenFactory: () => user.token
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection.start().catch(error => console.log(error));

        this.hubConnection.on('ReceiveMessageThread', messages => {
            this.messageThreadSource.next(messages)
        });

        this.hubConnection.on('UpdatedGroup', (group: Group) => {
            if (group.connections.some(x => x.username === otherUsername)) {
                this.messageThread$.pipe(take(1)).subscribe({
                    next: messages => {
                        messages.forEach(message => {
                            if(!message.dateRead) {
                                message.dateRead = new Date(Date.now())
                            }
                        })
                        this.messageThreadSource.next([...messages]);
                    }
                })
            }
        })

        this.hubConnection.on("NewMessage", message =>{
            this.messageThread$.pipe(take(1)).subscribe({
                next: messages => {
                    this.messageThreadSource.next([...messages, message]) // creates a new updated array with the existing messages (by spread), adding the new message sent.
                }
            })
        })

    }

    stopHubConnection() {
        if(this.hubConnection){
            this.hubConnection?.stop();
        }
    }

    getMessages(messageParams: MessageParams) {

        let params = getPaginationHeaders(messageParams);

        return getPaginatedResult<Message[]>(this.baseApiUrl + 'messages', params, this.http);
    }

    getMessageThread(username: string) {

        return this.http.get<Message[]>(this.baseApiUrl + 'messages/thread/'+username);
    }

    async sendMessage(username: string, content: string) {
        return this.hubConnection?.invoke('SendMessage', {recipientUsername: username, content: content})
            .catch(error => console.log(error));
    }

    deleteMessage(id: number) {
        return this.http.delete<Message>(this.baseApiUrl + `messages/${id}`);
    }
}
