import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';
import { MessageParams } from '../_models/messageParams';

@Component({
    selector: 'app-messages',
    templateUrl: './messages.component.html',
    styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
    
    messages?: Message[];
    pagination?: Pagination;
    messageParams: MessageParams;
    container: string = 'Unread';
    loading: boolean = false;

    constructor(private messageService: MessageService) { 

        this.messageParams = new MessageParams();
        this.messageParams.container = this.container;
    }

    ngOnInit(): void {
        this.loadMessages();
    }

    loadMessages() {

        this.loading = true;
        this.messageParams.container = this.container;
        this.messageService.getMessages(this.messageParams).subscribe({
            next: (response) => {
                this.messages = response.result;
                this.pagination = response.pagination;
                this.loading = false;
            }
        })
    }

    deleteMessage(id: number, event: any) {
        event.stopPropagation(); // prevents the route change in the process, mantaining the state of the page.
        this.messageService.deleteMessage(id).subscribe({
            next: () => {
                this.messages?.splice(this.messages.findIndex(m => m.id === id), 1)
            }
        });
    }

    pageChanged(event: any) {

        if(this.messageParams.pageNumber != event.page) {
            this.messageParams.pageNumber = event.page;
            this.loadMessages();
        }
    }
}
