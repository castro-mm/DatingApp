import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';
import { MessageParams } from '../_models/messageParams';

@Injectable({
    providedIn: 'root'
})
export class MessageService {

    baseApiUrl: string = environment.apiUrl;

    constructor(private http: HttpClient) { }

    getMessages(messageParams: MessageParams) {

        let params = getPaginationHeaders(messageParams);

        return getPaginatedResult<Message[]>(this.baseApiUrl + 'messages', params, this.http);
    }

    getMessageThread(username: string) {

        return this.http.get<Message[]>(this.baseApiUrl + 'messages/thread/'+username);
    }

    sendMessage(username: string, content: string) {
        return this.http.post<Message>(this.baseApiUrl + 'messages', {recipientUsername: username, content: content});
    }

    deleteMessage(id: number) {
        return this.http.delete<Message>(this.baseApiUrl + `messages/${id}`);
    }
}
