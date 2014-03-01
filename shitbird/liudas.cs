working with liudas:


public void Setup() {
	//Examples of fluent API
    //API change per liudas i blame skrilex
	using(BloomFilter filter = BloomFilter.BestBeforeYouReach(25).WithConllisionProbabilityOf(0.1)) // maybe LimitedTo(25)
	{

	}

	using(BloomFilter filter = BloomFilter.ForExpected(25).WithConllisionProbability(0.1)
	{

	}

	using(BloomFilter filter = BloomFilter.ForExpected(25).WithConllisionProbability(0.1).Unmanaged();
	{

	}

	using(BloomFilter filter = BloomFilter.WithSize(25).WithConllisionProbability(0.1).Unmanaged();
	{

	}

	using(BloomFilter filter = BloomFilter.Default())
	{

	}

	using(BloomFilter filter = new BloomFilter()) //equivalent to above
	{

	}

	using(BloomFilter filter = BloomFilter.ForExpected(25).WithConllisionProbability(0.1).PersitentTo(@".\urlFilter.bf")) // vs PersistedTo
	{

	}

	using(BloomFilter filter = BloomFilter.ForExpected(25).WithConllisionProbability(0.1).OnTransientMemoryMap())
	{

	}

	using(BloomFilter filter = BloomFilter.ForExpected(25).WithConllisionProbability(0.1).PersistedTo(@".\urlFilter.bf"))
	{

	}



	//resize
	using(BloomFilter filter = BloomFilter.ForExpected(25).WithCollisionProbability(0.1).PersitentTo(@".\urlFilter.bf").WithAutoResizing())
	{

	}
}