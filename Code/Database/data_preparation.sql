insert into csd.Features
(
	ContactID,
	CustomerID,
	mailFrom,
	MailTo,
	Subject,
	QueueType,
	Body,
	Skillset,
	SkillsetID,
	RuleID,
	ClosedReason,
	Importance,
	OpenDuration
)
select
	contact,
	contact->Customer,
	contact->MailFrom,
	contact->MailTo,
	contact->OriginalSubject,
	contact->Queuetype->TextValue,
	substring(text, 0, 10000),
	contact->Skillset->Name,
	contact->Skillset,
	contact->Rule,
	contact->ClosedReason->Name,
	contact->Importance,
	contact->OpenDuration
from cls.Actions
where Source->TextValue = 'EMail from Customer'


-- Remove the very old < April20xx
delete from csd.Features
where contactid <= 9528115





update csd.Features As feat
set transferCount = transferAgg.countIDs,
    firstTransferID = transferAgg.minID,
    lastTransferID = transferAgg.maxID
from 
	(
		select contact,
			min(id) minID,
			max(id) maxID,
			count(id) countIDs
		from cls.Actions
		where source = 48
		group by contact
	) As transferAgg
where feat.contactID = transferAgg.contact



update csd.Features As feat
set mailbox = mailboxes.mailbox
from csd.features
    inner join 
    (
    select "name" _ '@' _ "domainname" as mailbox
    from cls.Inboxes
    ) mailboxes
    on csd.features.mailto like '%' _ mailboxes.mailbox _ '%'
where feat.ContactID = csd.features.ContactID
	
	

update csd.Features As feat
set FirstSkillset =
    substring(
        text,
        (charindex('Original Skillset:', substring(text, 0, 500)) + 19), 
        (charindex('Target Skillset:', substring(text, 0, 500)) - 86)
    )
from cls.Actions
where feat.FirstTransferID = cls.Actions.ID



update csd.Features As feat
set LastSkillset =
	substring(
		text,
		(charindex('Target Skillset:', substring(text, 0, 500)) + 17)
	)
from cls.Actions
where feat.LastTransferID = cls.Actions.ID


update csd.Features As feat
set LastSkillset = substring(LastSkillset, 0, length(LastSkillset))
where LastSkillset is not null

  
--update csd.Features As feat
--set body = substring(text, 0, 4000)
--from cls.Actions
--where Source=39

--update csd.Features As feat
--set CustomerID = Customer
--from cls.Contacts
--where feat.contactID = cls.Contacts.ID

select 
	ContactID,
	CustomerID,
	Skillset,
	FirstSkillset,
	LastSkillset,
	MailFrom,
	MailTo,
	Subject,
	Body,
	QueueType,
	FollowUp,
	FirstTransferID,
	LastTransferID
from csd.Features


update csd.Features As feat
set lastSkillsetID = cls.Skillsets.ID
from cls.Skillsets
where feat.lastSkillset = cls.Skillsets.Name


update csd.Features As feat
set firstSkillsetID = cls.Skillsets.ID
from cls.Skillsets
where feat.firstSkillset = cls.Skillsets.Name


--FollowUp
update csd.Features
set FollowUp = 1
where Subject like '%[<A_' _ CustomerID _ '>]%'



--Rule	
update csd.Features As feat
set RuleID = cls.Contacts.Rule
from cls.Contacts
where feat.contactID = cls.Contacts.ID


--ReplyCount	
insert into csd.temp(contactID, Value)
select contact, count(ID) counter
from cls.Actions
where source->TextValue = 'EMail from Agent to Customer'
group by contact


update csd.Features As feat
set agentReply = Value
from csd.features
    inner join csd.temp on csd.features.contactID = csd.temp.contactID



--OpenDuration	
insert into csd.temp(contactID, Value)
select ID, OpenDuration
from cls.Contacts

update csd.Features As feat
set OpenDuration = Value
from csd.features
    inner join csd.temp on csd.features.contactID = csd.temp.contactID

	
 
--Cleanup
delete 
from csd.Features
where mailbox is null



update csd.Features
set MailCCCount = (len(mailCC) - len(replace(mailCC, '@', '')))
from csd.Features f
    inner join cls.Contacts c on f.contactid = c.id

update csd.Features
set MailToCount = (len(MailTo) - len(replace(MailTo, '@', '')))
from csd.Features f

update csd.Features
set spam = case
		when closedReason='Spam' then 'Spam'
		else 'NoSpam'
	end
	

	
-- Convoluted way to update a column due to performance issues
-- tempdb expansion
update csd.Features As feat
set NewCustomer = 0

insert into csd.temp(contactID, value)
select cls.Contacts.ID, datediff('second', cls.Contacts.Arrivaltime, cls.Customers.registerdate)
from cls.Contacts
    inner join cls.Customers on cls.Contacts.Customer = cls.Customers.ID


delete from csd.temp where value < -900
	
update csd.Features As feat
set NewCustomer = 1
where contactid in (select contactid from csd.temp)







update csd.Features
set spam='Spam'
where messagecleantagged like 'An email sent to you was quarantined%'



select count(id), spam
from csd.Features
where subject like '%linkedin%'
group by spam
 
616     NOSPAM
514     SPAM

update csd.Features
set spamoverride = 'Spam'
where subject like '%linkedin%'



3.      select id, subject, Spam
        from csd.Features
        where subject like '%linkedin%'
        and subject <> 'invitation to connect on linkedin'
 
/*
23377   Invito a collegarsi su LinkedIn         Spam
31117   Kontaktförfrågan på LinkedIn            Spam
59156   Invitación a conectarnos en LinkedIn    Spam
67282   Faça parte da minha rede no LinkedIn    Spam
82587   Reset Your LinkedIn Password            NoSpam
83221   Reset Your LinkedIn Password            Spam
90285   Millions of LinkedIn passwords stolen   Spam
90981   Invitación a conectarnos en LinkedIn    NoSpam
102103  40,000 leads from...LinkedIn???         NoSpam
134572  Invito a collegarsi su LinkedIn         NoSpam
174914  Invitation à se connecter sur LinkedIn  NoSpam
189287  Invitación a conectarnos en LinkedIn    NoSpam
199356  Invitación a conectarnos en LinkedIn    Spam
199776  Faça parte da minha rede no LinkedIn    Spam
202605  Invitation à se connecter sur LinkedIn  NoSpam
216216  Invitación a conectarnos en LinkedIn    NoSpam
232379  Join my network on LinkedIn             NoSpam
237605  Invitación a conectarnos en LinkedIn    NoSpam
261706  Invitación a conectarnos en LinkedIn    NoSpam
282355  Invitation à se connecter sur LinkedIn  Spam
288280  Invitación a conectarnos en LinkedIn    Spam
296547  Invitación a conectarnos en LinkedIn    NoSpam
*/
