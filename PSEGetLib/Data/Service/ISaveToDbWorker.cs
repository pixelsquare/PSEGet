using PSEGetLib.DocumentModel;

namespace PSEGetLib.Data.Service
{
	
	public interface ISaveToDbWorker
	{
		void Execute(PSEDocument pseDocument);
	}
	
}

