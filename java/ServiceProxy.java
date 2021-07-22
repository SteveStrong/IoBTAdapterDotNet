/*-------------------------------------------------------------------------------------------------
| *** UNCLASSIFIED ***
|--------------------------------------------------------------------------------------------------
| U.S. Army Research, Development, and Engineering Command
| Aviation and Missile Research, Development, and Engineering Center
| Software Engineering Directorate, Redstone Arsenal, AL
|--------------------------------------------------------------------------------------------------
| Export-Control Act Warning: Warning - This Document contains technical data whose export is
| restricted by the Arms Export Control Act (Title 22, U.S.C., Sec 2751, et seq) or the Export
| Administration Act of 1979, as amended, Title 50, U.S.C., App. 2401 et seq. Violations of these
| export laws are subject to severe criminal penalties. Disseminate in accordance with provisions
| of DoD Directive 5230.25.
|--------------------------------------------------------------------------------------------------
| Distribution Statement C:  Distribution authorized to U.S. Government Agencies and their
| contractors, Export Controlled, Critical Technology, 13 Feb 2017. Other request for this document
| shall be referred to U.S. Army Aviation and Missile Research Development and Engineering Center
| Software Engineering Directorate, Attn: RDMR-BAW, Redstone Arsenal, AL 35898.
--------------------------------------------------------------------------------------------------*/

package mil.airforce.mc2.sdk.common.proxy;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.atomic.AtomicBoolean;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.StatusRuntimeException;
import mil.airforce.mc2.logging.IDonLogging;
import mil.airforce.mc2.logging.LoggingFactory;
import mil.airforce.mc2.rx.IRxDisposable;
import mil.airforce.mc2.sdk.common.ClientCredentials;

import static java.util.concurrent.TimeUnit.SECONDS;

/**
 * Base class for proxies to SDK services that creates a channel that can be used by SDK Clients when connecting to the
 * SDK Server.
 */
abstract class ServiceProxy implements IRxDisposable
{
  private static final IDonLogging logger = LoggingFactory.getLogger(ServiceProxy.class);

  private final String serverAddress;
  private final int serverPort;
  protected final ClientCredentials credentials;

  protected ManagedChannel channel;
  protected AtomicBoolean isDisposed = new AtomicBoolean(false);

  /**
   * Creates a ServiceProxy
   *
   * @param host        the address of the SDK server
   * @param port        the port to use when connecting to the SDK server
   * @param credentials authentication/authorization credentials for the SDK client using the channel connected to the
   *                    SDK server.
   */
  ServiceProxy(String host, int port, ClientCredentials credentials)
  {
    this.serverAddress = host;
    this.serverPort = port;
    this.credentials = credentials;
  }

  /**
   * Initializes class after dependencies have been injected
   */
  protected void init()
  {
    channel = getChannel();
  }

  /**
   * Name for this service proxy
   *
   * @return service name
   */
  public abstract String getName();

  /**
   * Returns the channel used to connect to the SDK server.
   *
   * @return the channel used to connect to the SDK server.
   */
  protected ManagedChannel getChannel()
  {
    logger.i(getName() + " is using an unsecured channel.");
    return ManagedChannelBuilder.forAddress(serverAddress, serverPort)
        .usePlaintext()
        .build();
  }

  /**
   * Returns the address of the SDK server
   *
   * @return the address of the SDK server
   */
  protected String getServerAddress()
  {
    return serverAddress;
  }

  /**
   * Returns the port used when connecting to the SDK server
   *
   * @return the port used when connecting to the SDK server
   */
  protected int getServerPort()
  {
    return serverPort;
  }

  /**
   * The blocking call to verify that the service is up and available
   */
  abstract void makeBlockingConnectionCall();

  /**
   * we want to wait around until the SDK is running if it is not.. we do this by try to call getVersion()
   *
   * @param future the CompletableFuture object that can be cancelled
   */
  protected void blockUntilConnected(CompletableFuture<IRxDisposable> future)
  {
    boolean connected = false;
    while (!connected)
    {
      try
      {
        makeBlockingConnectionCall();
        connected = true;
      }
      catch (StatusRuntimeException ex)
      {
        try
        {
          Thread.sleep(1000);
        }
        catch (InterruptedException e)
        {
          Thread.currentThread().interrupt();
          logger.e("subscribe:: operation cancelled", e);
          future.cancel(false);
          break;
        }
      }
    }
  }

  /**
   * Close our channel to the service on disposal
   */
  @Override
  public void dispose()
  {
    if (!isDisposed())
    {
      logger.w(this.getName() + " disposing");
      isDisposed.set(true);
      closeChannel();
    }
  }

  @Override
  public boolean isDisposed()
  {
    return isDisposed.get();
  }

  /**
   * Close the connection to our proxy service
   */
  private void closeChannel()
  {
    try
    {
      channel.shutdown().awaitTermination(2, SECONDS);
    }
    catch (InterruptedException e)
    {
      logger.w("Error during channel shutdown: " + e.getMessage());
      Thread.currentThread().interrupt(); // Restore interrupted state.
    }
    catch (Exception e)
    {
      logger.w("Unexpected error during channel shutdown: " + e.getMessage());
    }
  }
}
