param([string]$src_qname, [string]$dest_qname)

if ( [System.String]::IsNullOrEmpty($src_qname) )
{
    write-host "You must provide a source queue name"
    exit
}

if ( [System.String]::IsNullOrEmpty($dest_qname) )
{
    write-host "You must provide a destination source queue name"
    exit
}

[Reflection.Assembly]::LoadWithPartialName("System.Messaging") | out-null

$src_q = new-object System.Messaging.MessageQueue($src_qname)
$dest_q = new-object System.Messaging.MessageQueue($dest_qname)

$msgs = $src_q.GetAllMessages()
$count = $msgs.Length

$cmd = read-host -prompt "About to move $count messages, enter c to confirm"

if ( $cmd -eq "c" )
{
    for( $i = 0; $i -lt $count; $i++ ) 
    {
        $msg = $src_q.Receive()
        $dest_q.Send( $msg, [System.Messaging.MessageQueueTransactionType]::Single )
        $lable = $msg.Label
        write-host "Message sent with label $label to $dest_qname"
    }
}

exit