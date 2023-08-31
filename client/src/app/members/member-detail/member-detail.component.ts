import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabDirective, TabsModule, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { FormsModule } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, FormsModule, TimeagoModule, MemberMessagesComponent]
})
export class MemberDetailComponent implements OnInit {

    @ViewChild('membersTabs', {static: true}) membersTabs?: TabsetComponent;
    acitveTab?: TabDirective;
    member: Member = {} as Member;
    images: GalleryItem[] = [];
    messages: Message[] = [];

    constructor(private memberService: MembersService, private route: ActivatedRoute, private messageService: MessageService) { }

    ngOnInit(): void {

        this.route.data.subscribe({
            next: (data) => {
                return this.member = data['member'];
            }
        })

        this.route.queryParams.subscribe({
            next: (params) => {
                params['tab'] && this.selectTab(params['tab'])
            }
        })

        this.getImages();
    }

    onTabActivated(data: TabDirective) {
        this.acitveTab = data;
        if(this.acitveTab.heading === 'Messages') {
            this.loadMessages();
        }
    }

    selectTab(heading: string) {
        if(this.membersTabs) {
            this.membersTabs.tabs.find(x => x.heading === heading)!.active = true;
        }
    }

    loadMessages() {
        if(this.member) {
            this.messageService.getMessageThread(this.member.userName).subscribe({
                next: (messages) => this.messages = messages
            })
        }
    }
 
    getImages() {
        if (!this.member) return;

        for (const photo of this.member.photos) {
            this.images.push(new ImageItem({src: photo.url, thumb: photo.url}));
        }
    }
}
